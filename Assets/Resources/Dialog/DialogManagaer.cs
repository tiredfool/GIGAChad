using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI diedText;
    public Image portraitImage;
    public GameObject dialogueBox;
    public GameObject blackBox; // 배경 이미지 표시에도 사용
    public TextMeshProUGUI blackText;
    public string jsonFileName = "Dialog/dialogues";
    public float typingSpeed = 0.05f;
    public float fontSizeIncrease = 5f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    public Slider slider;

    private int dialogueIndex = 0;
    public bool isTyping = false;
    private bool isBlackBoxActive = false;
    private Color originalBlackBoxColor; // blackBox의 초기 색상 저장
    private bool dialogueStarted = false;

    private Vector3 originalDialogueBoxPosition;

    private List<DialogueData> allDialogues = new List<DialogueData>();
    private List<DialogueData> currentDialogues = new List<DialogueData>();
    public List<Sprite> portraitSprites = new List<Sprite>();
    public PlayerController playerController;
    public FollowPlayer follower;

    public Image standingImageLeft;
    public Image standingImageRight;
    public List<Sprite> standingSprites = new List<Sprite>();

    private DialogueSequenceController[] sequenceControllers;

    public Sprite diedMessageBackground;
    private bool died = false;

    public float fadeDuration = 0.7f; // 배경이 완전히 투명해지는 데 걸리는 시간
    private Coroutine currentFadeCoroutine; // 현재 실행 중인 페이드 코루틴을 저장

    private Image blackBoxImage; // blackBox에 붙어있는 Image 컴포넌트
    private Coroutine currentBlackFadeCoroutine; // 현재 진행 중인 페이드 코루틴 참조

    // 화면 흔들림 코루틴의 참조를 저장할 변수 추가 (가장 중요)
    private Coroutine currentScreenShakeCoroutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        originalDialogueBoxPosition = dialogueBox.transform.localPosition;
        SetMaxHealth(100);
        diedText.text = "";
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);

        if (blackBox != null)
        {
            blackBoxImage = blackBox.GetComponent<Image>();
            if (blackBoxImage == null)
            {
                Debug.LogError("blackBox GameObject에 Image 컴포넌트가 없습니다!", blackBox);
            }
            else
            {
                originalBlackBoxColor = blackBoxImage.color;
                Color C = blackBoxImage.color;
                C.a = 0f;
                blackBoxImage.color = C;
                blackBox.SetActive(false); // 일단 비활성화
            }
        }
        else
        {
            Debug.LogError("DialogueManager에 blackBox GameObject가 할당되지 않았습니다!");
        }
        // originalBlackBoxColor는 blackBoxImage가 할당된 후에 설정하는 것이 더 안전합니다.
        // blackBoxImage가 null일 경우 오류가 발생할 수 있습니다.
        // 이미 blackBoxImage가 null이 아니라는 검사를 했으므로 이 위치는 괜찮습니다.

        LoadDialogueFromJson();

        
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        sequenceControllers = FindObjectsOfType<DialogueSequenceController>();
        Debug.Log($"찾은 DialogueSequenceController 개수: {sequenceControllers.Length}");
    }
    void Start()
    {
        // Start에서는 할 게 없음
    }


    // 씬이 로드될 때마다 호출되는 이벤트 핸들러 등록
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 씬이 언로드될 때 호출되는 이벤트 핸들러 해제
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되는 메서드
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}, 모드: {mode}");

        // 씬이 로드될 때마다 playerController와 follower를 다시 찾아서 할당합니다.
        playerController = FindObjectOfType<PlayerController>();
        follower = FindObjectOfType<FollowPlayer>();
        follower.SetVisible(false);
        if (playerController == null)
        {
            Debug.LogWarning("씬에서 PlayerController를 찾을 수 없습니다.");
        }
        if (follower == null)
        {
            Debug.LogWarning("씬에서 FollowPlayer를 찾을 수 없습니다.");
        }
        else
        {
            follower.SetVisible(false); // 초기 상태는 숨기기
        }

        // DialogueSequenceController도 씬마다 다를 수 있으므로 다시 찾아야 합니다.
        sequenceControllers = FindObjectsOfType<DialogueSequenceController>();
        Debug.Log($"OnSceneLoaded에서 찾은 DialogueSequenceController 개수: {sequenceControllers.Length}");

        // 초기 UI 상태 설정
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);

        // 사망 상태 초기화 (씬 전환 시)
        died = false;
        diedText.text = "";
        nameText.gameObject.SetActive(true); // 이름 텍스트 다시 활성화
        portraitImage.gameObject.SetActive(true); // 초상화 다시 활성화
        // blackBox 이미지 상태 초기화 (사망 메시지 배경 남아있을 수 있으므로)
        if (blackBoxImage != null)
        {
            blackBoxImage.sprite = null;
            blackBoxImage.color = originalBlackBoxColor;
        }

        // 현재 진행 중인 대화가 있다면 초기화
        dialogueStarted = false;
        isTyping = false;
        dialogueIndex = 0;
        currentDialogues.Clear(); // 이전 씬의 대화 목록 클리어

        // 기존의 모든 코루틴 중지 (특히 Time.timeScale=0 때문에 멈춰있을 수 있는 코루틴)
        StopAllCoroutines();
        // 하지만 StopAllCoroutines()은 이 스크립트의 모든 코루틴을 중지시키므로,
        // 필요에 따라 개별적인 Coroutine 변수를 이용해 StopCoroutine()을 사용하는 것이 좋습니다.
        // 예를 들어, FadeCoroutine이나 ShakeScreen 코루틴은 씬 전환 시에도 문제가 될 수 있으므로,
        // EndDialogue나 OnSceneLoaded에서 개별적으로 중지하는 로직을 강화할 수 있습니다.
        currentFadeCoroutine = null;
        currentBlackFadeCoroutine = null;
        currentScreenShakeCoroutine = null;

        // 혹시 Time.timeScale이 0으로 고정되어 있다면 다시 1로 돌려줍니다.
        // (대화 도중 씬이 로드될 경우 대비)
        Time.timeScale = 1f;
        follower.SetVisible(false);
    }

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(float health)
    {
        slider.value = health;
    }

    void LoadDialogueFromJson()
    {
        allDialogues.Clear();
        TextAsset textAsset = Resources.Load<TextAsset>(jsonFileName);
        if (textAsset == null)
        {
            Debug.LogError("JSON 파일 '" + jsonFileName + "'을 찾을 수 없습니다.");
            return;
        }

        string jsonString = textAsset.text;
        DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(jsonString);
        allDialogues = new List<DialogueData>(wrapper.dialogues);

        Debug.Log("Loaded " + allDialogues.Count + " dialogues from JSON.");
    }

    public void SetDialogues(List<DialogueData> dialogues)
    {
        Debug.Log($"SetDialogues 호출됨. 받은 대화 개수: {dialogues.Count}");
        currentDialogues = dialogues;
        dialogueIndex = 0;
        Debug.Log($"SetDialogues 완료. currentDialogues 개수: {currentDialogues.Count}, dialogueIndex: {dialogueIndex}");
    }

    public void StartDialogue()
    {
        Debug.Log("StartDialogue 호출됨");
        if (currentDialogues.Count == 0)
        {
            Debug.LogWarning("No dialogues set for this scene!");
            return;
        }
        follower.SetVisible(true);
        dialogueIndex = 0;
        dialogueBox.SetActive(true);
        playerController.SetTalking(true);
        Time.timeScale = 0f;
        follower.togleLocate();
        Debug.Log($"StartDialogue: 첫 번째 대사 표시 시도 (index: {dialogueIndex}, ID: {currentDialogues[dialogueIndex].id})");

        ShowDialogue(currentDialogues[dialogueIndex]);
        dialogueIndex++;
        dialogueStarted = true;
        Debug.Log("StartDialogue 완료");
    }

    void ShowDialogue(DialogueData data)
    {
        // 현재 실행 중인 모든 코루틴을 중지시키는 대신, 필요한 코루틴만 중지하도록 변경
        // StartCoroutine(TypeDialogue)는 여기서 다시 시작되므로 걱정 없음
        StopExistingCoroutines(); // 새로 추가된 함수 호출
        
        // 모든 대화 박스 및 관련 요소 비활성화 (배경 이미지 포함)
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // 색상 초기화 (페이드인 시에는 알파를 0으로 설정)
      //  if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
      //  if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        isBlackBoxActive = false;
        if (data.id == "E-A")
        {
           
            Debug.Log("페이드 아웃 시작");
            // 오른쪽 스탠딩 이미지 페이드 아웃
            if (standingImageRight != null) // .gameObject.activeInHierarchy는 이미지를 다시 활성화할 것이므로 제거하거나, 활성 상태를 먼저 확인
            {
                // 페이드 아웃 시작 전에 일단 활성화 상태를 보장 (새로 나타나는 경우 대비)
                if (!standingImageRight.gameObject.activeInHierarchy)
                {
                    standingImageRight.gameObject.SetActive(true);
                    standingImageRight.color = new Color(standingImageRight.color.r, standingImageRight.color.g, standingImageRight.color.b, 1f); // 초기 알파값 1로 설정
                }
                StartCoroutine(FadeOutImage(standingImageRight, fadeDuration));
            }
            else
            {
                Debug.LogWarning("[ShowDialogue] standingImageRight가 할당되지 않았습니다. 페이드 아웃 불가.");
            }


            // follower 페이드 아웃 (FollowPlayer 스크립트에 페이드 관련 로직이 필요할 수 있습니다)
            if (follower != null)
            {
                SpriteRenderer followerSpriteRenderer = follower.GetComponent<SpriteRenderer>();
                if (followerSpriteRenderer != null)
                {
                    // 페이드 아웃 시작 전에 일단 활성화 상태를 보장
                    if (!followerSpriteRenderer.gameObject.activeInHierarchy)
                    {
                        followerSpriteRenderer.gameObject.SetActive(true);
                        followerSpriteRenderer.color = new Color(followerSpriteRenderer.color.r, followerSpriteRenderer.color.g, followerSpriteRenderer.color.b, 1f);
                    }
                    StartCoroutine(FadeOutSpriteRenderer(followerSpriteRenderer, fadeDuration));
                }
                else
                {
                    Debug.LogWarning("[ShowDialogue] follower에 SpriteRenderer가 없습니다. 페이드 아웃 불가.");
                    // SpriteRenderer가 없으면 즉시 비활성화 처리
                    if (follower.gameObject.activeInHierarchy) follower.SetVisible(false);
                }
            }
            else
            {
                Debug.LogWarning("[ShowDialogue] follower가 할당되지 않았습니다. 페이드 아웃 불가.");
            }
        }
        else // "E-A"가 아닐 경우, 이미지들이 활성화되어야 할 수 있으므로 ShowDialogue의 뒷부분에서 처리
        {
            if (standingImageRight != null) standingImageRight.gameObject.SetActive(false); // 기본 상태는 비활성화
            if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false); // 기본 상태는 비활성화
                                                                                          // follower도 마찬가지로, 필요에 따라 여기서 비활성화하거나,
                                                                                          // ShowDialogue의 일반 로직에서 다시 활성화되도록 처리
        }
        if (data.dialogueType == "normal")
        {
            dialogueBox.SetActive(true);
            nameText.text = data.npcName;

            Sprite portrait = portraitSprites.Find(s => s.name == data.portraitName);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
            }

            // 배경 이미지 처리 (Image 컴포넌트 사용)
            if (!string.IsNullOrEmpty(data.backgroundImageName))
            {
                Sprite backgroundImage = Resources.Load<Sprite>("Backgrounds/" + data.backgroundImageName);
                if (backgroundImage != null)
                {
                    blackBox.SetActive(true);
                    blackBoxImage.color = new Color(1, 1, 1, 1); // 배경 이미지를 원래 색상으로 보이게 (불투명)
                    blackBoxImage.sprite = backgroundImage; // Image 컴포넌트에 스프라이트 할당
                }
                else
                {
                    Debug.LogError("Background image not found: " + data.backgroundImageName);
                }
            }

            // 스탠딩 일러스트 처리 (기존과 동일)
            string[] standingImages = data.standingImage?.Split(',');
            string[] standingPositions = data.standingPosition?.Split(',');

            if (standingImages != null)
            {
                for (int i = 0; i < standingImages.Length; i++)
                {
                    string imageName = standingImages[i].Trim();
                    if (!string.IsNullOrEmpty(imageName))
                    {
                        Sprite standingSprite = standingSprites.Find(s => s.name == imageName);
                        if (standingSprite != null)
                        {
                            string position = (standingPositions != null && i < standingPositions.Length) ? standingPositions[i].Trim().ToLower() : "";

                            if (position == "left" && standingImageLeft != null)
                            {
                                standingImageLeft.gameObject.SetActive(true);
                                standingImageLeft.sprite = standingSprite;
                                standingImageLeft.color = Color.white;
                            }
                            else if (position == "right" && standingImageRight != null && data.id != "E-A")
                            {
                                standingImageRight.gameObject.SetActive(true);
                                standingImageRight.sprite = standingSprite;
                                standingImageLeft.color = Color.white;
                            }
                            else if (position == "both" && standingImageLeft != null && standingImageRight != null)
                            {
                                standingImageLeft.gameObject.SetActive(true);
                                standingImageLeft.sprite = standingSprite;
                                standingImageRight.gameObject.SetActive(true);
                                standingImageRight.sprite = standingSprite;
                            }
                            else if (!string.IsNullOrEmpty(position) && position != "both")
                            {
                                Debug.LogError("Invalid standing position: " + position);
                            }
                        }
                        else
                        {
                            Debug.LogError("Standing sprite not found: " + imageName);
                        }
                    }
                }
            }

            // 흔들림이 필요한 경우에만 코루틴 시작, 참조 저장
            if (data.shakeScreen)
            {
                currentScreenShakeCoroutine = StartCoroutine(ShakeScreen(data.portraitName));
            }
            StartCoroutine(TypeDialogue(data, dialogueText));
        }
        else if (data.dialogueType == "black")
        {
            blackBoxImage.color = new Color(0, 0, 0, 1);
            blackBox.SetActive(true);
            isBlackBoxActive = true;
            // Black 타입 대화에서도 TypeDialogue는 시작해야 함
            StartCoroutine(TypeDialogue(data, blackText));

            // Black 박스 지속 시간 후 다음 대사 진행
            StartCoroutine(WaitForBlackBoxEnd(data.blackBoxDuration));
        }
        else
        {
            Debug.LogError("Unknown dialogue type: " + data.dialogueType);
        }
    }

    // 새로운 대화가 시작될 때 기존 코루틴들을 안전하게 중지시키는 함수
    void StopExistingCoroutines()
    {
        // 기존의 화면 흔들림 코루틴이 있다면 중지
        if (currentScreenShakeCoroutine != null)
        {
            StopCoroutine(currentScreenShakeCoroutine);
            currentScreenShakeCoroutine = null;
            // DialogueManager에서 흔들림을 멈출 때 FollowPlayer에게도 흔들림을 멈추라고 지시
            if (follower != null)
            {
                follower.SetShake(false);
            }
            dialogueBox.transform.localPosition = originalDialogueBoxPosition; // 대화 박스 위치 원복
        }
        // 이 외에 TypeDialogue, WaitForBlackBoxEnd 등은 새로운 ShowDialogue에서 다시 시작되므로 명시적으로 중지할 필요가 없거나,
        // 필요하다면 개별 참조를 통해 중지해야 합니다.
        // 현재 코드에서는 TypeDialogue는 다시 시작되고, WaitForBlackBoxEnd는 blackBoxActive로 제어되므로 괜찮습니다.
    }


    IEnumerator WaitForBlackBoxEnd(float duration)
    {
        Debug.Log("WaitForBlackBoxEnd 시작: " + duration + "초 대기");
        yield return new WaitForSecondsRealtime(duration);
        Debug.Log("WaitForBlackBoxEnd 종료, NextDialogue 호출 시도 (isTyping: " + isTyping + ")");

        // 타이핑이 완료될 때까지 대기
        while (isTyping)
        {
            Debug.Log("WaitForBlackBoxEnd: 아직 타이핑 중... 대기");
            yield return null; // 다음 프레임까지 대기
        }

        Debug.Log("WaitForBlackBoxEnd: 타이핑 완료됨, NextDialogue 호출");
        NextDialogue();
    }


    IEnumerator TypeDialogue(DialogueData data, TextMeshProUGUI T)
    {
        isTyping = true;
        T.fontSize = data.fontSize;
        T.text = "";

        foreach (char letter in data.dialogue.ToCharArray())
        {
            T.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        Debug.Log("타이핑 완료: isTyping = false"); // 타이핑 완료 시 로그
    }

    public void SetDiedMessage(string message)
    {
        if (diedText != null)
        {
            died = true;
            dialogueBox.SetActive(false);
            if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
            if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
            nameText.gameObject.SetActive(false); // 이름 텍스트 숨기기
            portraitImage.gameObject.SetActive(false); // 초상화 숨기기

            blackBox.SetActive(true); //

            Image blackBoxImage = blackBox.GetComponent<Image>();
            if (blackBoxImage != null)
            {
                blackBoxImage.color = new Color(1, 1, 1, 0);

                if (diedMessageBackground != null) // 인스펙터에 할당된 스프라이트가 있는지 확인
                {
                    blackBoxImage.sprite = diedMessageBackground; // Image 컴포넌트에 할당
                }
                else
                {
                    blackBoxImage.sprite = null; // Clear any previous sprite
                    blackBoxImage.color = new Color(0, 0, 0, 0);  // 완전 투명 검은색 (배경 이미지 없으면)
                }

                // 기존 페이드 코루틴이 있다면 중지
                if (currentFadeCoroutine != null)
                {
                    StopCoroutine(currentFadeCoroutine);
                }
                currentFadeCoroutine = StartCoroutine(FadeInDiedBackgroundAndShowText(blackBoxImage, message));
            }
            else
            {
                Debug.LogError("blackBox GameObject에 UI.Image 컴포넌트가 없어 DiedMessageBackground를 설정할 수 없습니다!");
            }

            isBlackBoxActive = true; // blackBox가 활성화되었음을 표시 
        }
        else
        {
            Debug.LogError("diedText가 DialogueManager Inspector 창에 할당되지 않았습니다!");
        }
    }

    IEnumerator FadeInDiedBackgroundAndShowText(Image targetImage, string message)
    {
        float timer = 0f;
        Color startColor = targetImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Time.timeScale이 0일 때도 작동
            float progress = timer / fadeDuration;
            targetImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        targetImage.color = endColor;

        diedText.gameObject.SetActive(true); // diedText 활성화
        diedText.text = ""; // 텍스트 초기화 
        // StopAllCoroutines(); // 이 부분은 TypeDialogue만 중지하는 것이 좋을 수 있습니다.
        // 현재는 diedText 타이핑만 시작하므로 괜찮습니다.
        StartCoroutine(TypeDialogue(new DialogueData { dialogue = message, fontSize = diedText.fontSize }, diedText));
    }

    IEnumerator ShakeScreen(string name) // string name 파라미터는 data.portraitName에서 가져옵니다.
    {
        float elapsed = 0.0f;
        // 특정 인물일 때만 카메라 흔들림 활성화
        if (name == "Gigachard" || name == "Gigachard_ex")
            follower.SetShake(true);

        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;

            // 대화 박스도 흔들리게 하려면 이 줄 유지
            dialogueBox.transform.localPosition = originalDialogueBoxPosition + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime; // UnscaledDeltaTime 사용

            yield return null;
        }

        // 흔들림이 끝나면 대화 박스 위치 원복
        dialogueBox.transform.localPosition = originalDialogueBoxPosition;
        // 특정 인물일 때만 카메라 흔들림 비활성화
        if (name == "Gigachard" || name == "Gigachard_ex")
            follower.SetShake(false);

        // 코루틴이 자연스럽게 종료되었으므로 참조를 null로 설정
        currentScreenShakeCoroutine = null;
    }

    public void NextDialogue()
    {
        if (isTyping) // 타이핑 중이면 스킵 방지
        {
            Debug.Log("NextDialogue: 타이핑 중이므로 스킵");
            // 현재 타이핑 중인 대화를 즉시 완료하는 로직을 추가할 수 있습니다.
            // dialogueText.text = currentDialogues[dialogueIndex - 1].dialogue;
            // isTyping = false;
            return;
        }

        string completedDialogueId = (dialogueIndex > 0 && dialogueIndex <= currentDialogues.Count) ? currentDialogues[dialogueIndex - 1].id : "";

        // 모든 시퀀스 컨트롤러에 방금 완료된 대화 ID 전달
        foreach (var controller in sequenceControllers)
        {
            controller.CheckDialogueEnd(completedDialogueId);
            if (controller.cameraController != null && controller.cameraController.IsMoving)
            {
                Debug.Log($"카메라 이동 중, 다음 대사 진행 보류 (Controller: {controller.gameObject.name})");
                return;
            }
        }

        // --- 여기까지 카메라 전환 로직 ---

        if (dialogueIndex < currentDialogues.Count)
        {
            Debug.Log($"NextDialogue: 다음 대사 표시 시도 (index: {dialogueIndex}, ID: {currentDialogues[dialogueIndex].id})");

            ShowDialogue(currentDialogues[dialogueIndex]);
            dialogueIndex++;
        }
        else
        {
            Debug.Log("NextDialogue: 대화 끝");
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialogueBox.SetActive(false);
        if (!died)
        {
            blackBox.SetActive(false);

            Image blackBoxImage = blackBox.GetComponent<Image>();
            if (blackBoxImage != null)
            {
                blackBoxImage.sprite = null; // 사용했던 스프라이트 초기화
                blackBoxImage.color = originalBlackBoxColor; // 색상 원래대로
            }
            isBlackBoxActive = false;
        }
        // 대화 종료 시 모든 관련 코루틴을 안전하게 중지
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        if (currentBlackFadeCoroutine != null)
        {
            StopCoroutine(currentBlackFadeCoroutine);
            currentBlackFadeCoroutine = null;
        }
        StopExistingCoroutines(); // 대화 종료 시 흔들림 코루틴도 확실히 중지

        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        playerController.SetTalking(false);
        Time.timeScale = 1f;
        follower.togleLocate(); // 대화 종료 시 위치 반전? (이 로직이 맞는지는 확인 필요)
        follower.SetVisible(false);
        dialogueStarted = false;
        Debug.Log("대화 종료");
    }

    public List<DialogueData> GetDialogues()
    {
        return allDialogues;
    }

    void Update()
    {
        if (VirtualInputManager.Instance.GetKeyOrButton("Action") && !isBlackBoxActive)
        {
            Debug.Log("엔터눌림");
            NextDialogue();
        }
    }

    public void StartDialogueByIdRange(string startId, string endId)
    {
        Debug.Log($"StartDialogueByIdRange 호출됨 (startId: {startId}, endId: {endId}, dialogueStarted: {dialogueStarted})");
        if (dialogueStarted)
        {
            Debug.LogWarning("이미 대화가 진행 중입니다.");
            return;
        }

        List<DialogueData> sceneDialogues = GetDialoguesForScene(startId, endId);
        if (sceneDialogues != null && sceneDialogues.Count > 0)
        {
            SetDialogues(sceneDialogues);
            StartDialogue();
        }
        else
        {
            Debug.LogWarning($"ID '{startId}'부터 '{endId}'까지의 대화를 찾을 수 없습니다.");
        }
        Debug.Log("StartDialogueByIdRange 완료");
    }

    List<DialogueData> GetDialoguesForScene(string startId, string endId)
    {
        Debug.Log($"GetDialoguesForScene 호출됨 (startId: {startId}, endId: {endId})");
        List<DialogueData> sceneDialogues = new List<DialogueData>();
        bool inRange = false;

        List<DialogueData> allDialogues = GetDialogues();

        if (allDialogues == null || allDialogues.Count == 0)
        {
            Debug.LogError("DialogueManager에서 대화 데이터를 불러오지 못했습니다.");
            return null;
        }

        foreach (DialogueData dialogue in allDialogues)
        {
            Debug.Log($"현재 확인 중인 대사 ID: {dialogue.id}");
            if (dialogue.id == startId)
            {
                inRange = true;
                Debug.Log($"시작 대사 찾음 (ID: {dialogue.id})");
                sceneDialogues.Add(dialogue);
            }
            else if (inRange)
            {
                sceneDialogues.Add(dialogue);
                Debug.Log($"씬 대화 목록에 추가 (ID: {dialogue.id})");
            }

            if (dialogue.id == endId)
            {
                Debug.Log($"종료 대사 찾음 (ID: {dialogue.id}), 대화 목록 생성 완료");
                break;
            }
        }
        Debug.Log($"GetDialoguesForScene 완료. 찾은 대화 개수: {sceneDialogues.Count}");
        return sceneDialogues;
    }

    public void ReloadBlack()
    {
        blackBox.SetActive(false);
        Image blackBoxImage = blackBox.GetComponent<Image>();
        if (blackBoxImage != null)
        {
            blackBoxImage.sprite = null;
            blackBoxImage.color = originalBlackBoxColor;
        }
        // 모든 관련 페이드 코루틴 중지
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        if (currentBlackFadeCoroutine != null) StopCoroutine(currentBlackFadeCoroutine);
        currentFadeCoroutine = null;
        currentBlackFadeCoroutine = null;
        isBlackBoxActive = false;
    }

    public void FadeToBlack(Action onComplete = null)
    {
        if (blackBoxImage == null)
        {
            Debug.LogError("blackBoxImage가 초기화되지 않았습니다. FadeToBlack을 호출할 수 없습니다.");
            onComplete?.Invoke();
            return;
        }

        if (currentBlackFadeCoroutine != null)
        {
            StopCoroutine(currentBlackFadeCoroutine);
        }
        currentBlackFadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    public void FadeFromBlack(Action onComplete = null)
    {
        if (blackBoxImage == null)
        {
            Debug.LogError("blackBoxImage가 초기화되지 않았습니다.");
            onComplete?.Invoke();
            return;
        }

        if (currentBlackFadeCoroutine != null)
        {
            StopCoroutine(currentBlackFadeCoroutine);
        }
        blackBoxImage.sprite = null;
        currentBlackFadeCoroutine = StartCoroutine(FadeCoroutine(blackBoxImage.color.a, 0f, onComplete));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, Action onComplete)
    {
        blackBox.SetActive(true);

        float timer = 0f;
        Color currentColor = blackBoxImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            currentColor.a = alpha;
            blackBoxImage.color = currentColor;
            yield return null;
        }

        currentColor.a = endAlpha;
        blackBoxImage.color = currentColor;

        if (endAlpha == 0f)
        {
            blackBox.SetActive(false);
        }

        currentBlackFadeCoroutine = null;
        onComplete?.Invoke();
    }

    public void BlackState(bool onOff)
    {
        if (onOff)
        {
            FadeToBlack();
        }
        else
        {
            FadeFromBlack();
        }
    }

    public void setBlack()
    {
        if (blackBoxImage != null)
        {
            blackBoxImage.sprite = null;
            blackBoxImage.color = new Color(0f, 0f, 0f, 1f);
            blackBox.SetActive(true);
            // 기존 코루틴 중지 로직을 여기에 포함
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            if (currentBlackFadeCoroutine != null) StopCoroutine(currentBlackFadeCoroutine);
            currentFadeCoroutine = null;
            currentBlackFadeCoroutine = null;
            isBlackBoxActive = true;
        }
    }
    public void Flash(Action onComplete = null)
    {
        if (blackBoxImage == null)
        {
            Debug.LogError("blackBoxImage가 초기화되지 않았습니다. Flash를 호출할 수 없습니다.");
            onComplete?.Invoke();
            return;
        }

        // 기존 페이드 코루틴이 있다면 중지
        if (currentBlackFadeCoroutine != null)
        {
            StopCoroutine(currentBlackFadeCoroutine);
        }

        // blackBox를 활성화하고 흰색으로 설정
        blackBox.SetActive(true);
        blackBoxImage.sprite = null; // 스프라이트가 있다면 지워줍니다.
        blackBoxImage.color = new Color(1f, 1f, 1f, 1f); // 완전 불투명한 흰색

        // 흰색에서 완전히 투명해지는 페이드 아웃 코루틴 시작
        currentBlackFadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, () => {
            // 페이드 아웃 완료 후 비활성화
            blackBox.SetActive(false);
            // 원래 색상으로 되돌리기 (다음 검은색 페이드인 등을 위해)
            blackBoxImage.color = originalBlackBoxColor;
            onComplete?.Invoke();
        }));
    }
    public IEnumerator FadeOutImage(Image targetImage, float duration, Action onComplete = null)
    {
        if (targetImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        Color startColor = targetImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 투명하게 페이드 아웃

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Time.timeScale이 0일 때도 작동하도록 unscaledDeltaTime 사용
            float progress = timer / duration;
            targetImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        targetImage.color = endColor; // 완전히 투명하게 설정
        targetImage.gameObject.SetActive(false); // 페이드 아웃 후 GameObject 비활성화 (선택 사항)
        onComplete?.Invoke();
    }// DialogueManager 클래스 내부에 추가
    public IEnumerator FadeOutSpriteRenderer(SpriteRenderer targetRenderer, float duration, Action onComplete = null)
    {
        if (targetRenderer == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        Color startColor = targetRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            targetRenderer.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        targetRenderer.color = endColor;
        targetRenderer.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
    public void off()
    {
        instance = null;
        Destroy(this.gameObject);
    }
}