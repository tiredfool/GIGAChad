using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

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
                // 시작 시 블랙박스 Image의 Alpha 값을 0으로 설정하여 투명하게 만듦 (필요에 따라)
                // 만약 시작 시 블랙박스가 항상 꺼진 상태라면 BlackState(false)를 호출해도 됨.
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
        originalBlackBoxColor = blackBoxImage.color; // 초기 색상 저장

        LoadDialogueFromJson();

        follower.SetVisible(false);
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        sequenceControllers = FindObjectsOfType<DialogueSequenceController>();
        Debug.Log($"찾은 DialogueSequenceController 개수: {sequenceControllers.Length}");
    }
    void Start()
    {
       
      
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
        Debug.Log($"StartDialogue: 첫 번째 대사 표시 시도 (index: {dialogueIndex}, ID: {currentDialogues[dialogueIndex].id})");
        ShowDialogue(currentDialogues[dialogueIndex]);
        dialogueIndex++;
        dialogueStarted = true;
        Debug.Log("StartDialogue 완료");
    }

    void ShowDialogue(DialogueData data)
    {
        // 모든 대화 박스 및 관련 요소 비활성화 (배경 이미지 포함)
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        // blackBox.GetComponent<SpriteRenderer>().enabled = false; // 이 줄 제거
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // 색상 초기화 (페이드인 시에는 알파를 0으로 설정)
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        isBlackBoxActive = false;

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
                    // 일반 대화의 배경 이미지는 바로 보이도록 알파 1로 설정
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
                            }
                            else if (position == "right" && standingImageRight != null)
                            {
                                standingImageRight.gameObject.SetActive(true);
                                standingImageRight.sprite = standingSprite;
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

            StopAllCoroutines();
            if (data.shakeScreen) StartCoroutine(ShakeScreen());
            StartCoroutine(TypeDialogue(data, dialogueText));
        }
        else if (data.dialogueType == "black")
        {
            blackBoxImage.color = new Color(0, 0, 0, 1);
            blackBox.SetActive(true);
            isBlackBoxActive = true;
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(data, blackText));

            // Black 박스 지속 시간 후 다음 대사 진행
            StartCoroutine(WaitForBlackBoxEnd(data.blackBoxDuration));
        }
        else
        {
            Debug.LogError("Unknown dialogue type: " + data.dialogueType);
        }
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
                    blackBoxImage.color = new Color(0, 0, 0, 0); 
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
        StopAllCoroutines(); // 혹시 모를 다른 타이핑 코루틴 중지 (이 함수가 시작될 때)
        StartCoroutine(TypeDialogue(new DialogueData { dialogue = message, fontSize = diedText.fontSize }, diedText));
       
        
    }

    IEnumerator ShakeScreen()
    {
        float elapsed = 0.0f;
        follower.SetShake(true);
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;

            dialogueBox.transform.localPosition = originalDialogueBoxPosition + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        dialogueBox.transform.localPosition = originalDialogueBoxPosition;
        follower.SetShake(false);
    }

    public void NextDialogue()
    {
        if (isTyping) // 타이핑 중이면 스킵 방지
        {
            Debug.Log("NextDialogue: 타이핑 중이므로 스킵");
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
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        playerController.SetTalking(false);
        Time.timeScale = 1f;
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
        if (Input.GetKeyDown(KeyCode.Return) && !isBlackBoxActive )
        {
            NextDialogue();
        }
    }

    public void StartDialogueByIdRange(string startId, string endId)// 외부에서 인스턴스로 불러와 대화 진행 가능
    {
        Debug.Log($"StartDialogueByIdRange 호출됨 (startId: {startId}, endId: {endId}, dialogueStarted: {dialogueStarted})");
        if (dialogueStarted) // 이미 대화가 진행 중이면 중복 시작 방지
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
                sceneDialogues.Add(dialogue); // 시작 대사를 찾으면 바로 추가
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
            blackBoxImage.sprite = null; // 이미지 컴포넌트 스프라이트 초기화
            blackBoxImage.color = originalBlackBoxColor; // 색상 원래대로
        }
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        isBlackBoxActive = false;
       
    }
    public void FadeToBlack(Action onComplete = null)
    {
        if (blackBoxImage == null)
        {
            Debug.LogError("blackBoxImage가 초기화되지 않았습니다. BlackState를 호출할 수 없습니다.");
            onComplete?.Invoke(); // 오류 시에도 콜백은 호출하여 멈추지 않게 함
            return;
        }

        if (currentBlackFadeCoroutine != null)
        {
            StopCoroutine(currentBlackFadeCoroutine);
        }
        currentBlackFadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, onComplete)); // 투명 -> 검은색
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
        blackBox.SetActive(true); // 페이드 시작 시 블랙박스 활성화 (투명 -> 검은색이든, 검은색 -> 투명 투명 시작 시)

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

        currentColor.a = endAlpha; // 최종 알파값 정확히 적용
        blackBoxImage.color = currentColor;

        // 투명해지면 비활성화 (검은색 -> 투명 연출일 때만)
        if (endAlpha == 0f)
        {
            blackBox.SetActive(false);
        }

        currentBlackFadeCoroutine = null; // 코루틴 완료
        onComplete?.Invoke(); // 연출 완료 후 콜백 호출
    }

    
    public void BlackState(bool onOff)
    {
       
        if (onOff)
        {
            FadeToBlack(); // 즉시 블랙박스 켜기 (페이드 인)
        }
        else
        {
            FadeFromBlack(); // 즉시 블랙박스 끄기 (페이드 아웃)
        }
    }
    public void setBlack()
    {
        if (blackBoxImage != null)
        {
            blackBoxImage.sprite = null; // 스프라이트 제거
            blackBoxImage.color = new Color(0f, 0f, 0f, 1f); // 완전 불투명 검은색
            blackBox.SetActive(true); // 활성화
                                      // 기존 코루틴 중지 로직도 여기에 포함되어야 함
            if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
            if (currentBlackFadeCoroutine != null) StopCoroutine(currentBlackFadeCoroutine);
            currentFadeCoroutine = null;
            currentBlackFadeCoroutine = null;
            isBlackBoxActive = true;
        }
    }
}
