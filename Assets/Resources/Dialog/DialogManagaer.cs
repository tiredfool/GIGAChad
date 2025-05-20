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
    private bool isTyping = false;
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
        originalBlackBoxColor = blackBox.GetComponent<Image>().color; // 초기 색상 저장
        LoadDialogueFromJson();

        follower.SetVisible(false);
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);

        // blackBox에 SpriteRenderer 컴포넌트가 없으면 추가
        if (blackBox.GetComponent<SpriteRenderer>() == null)
        {
            blackBox.AddComponent<SpriteRenderer>();
            blackBox.GetComponent<SpriteRenderer>().sortingOrder = -1; // 대화창 뒤에 표시되도록 설정 (조절 가능)
        }
        blackBox.GetComponent<SpriteRenderer>().enabled = false; // 초기에는 비활성화
        blackBox.GetComponent<Image>().enabled = true; // Image 컴포넌트는 활성화 (색상 제어용)
    }

    void Start()
    {
        // 씬 시작 시 모든 DialogueSequenceController 찾기
        sequenceControllers = FindObjectsOfType<DialogueSequenceController>();
        Debug.Log($"찾은 DialogueSequenceController 개수: {sequenceControllers.Length}");
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
        blackBox.GetComponent<SpriteRenderer>().enabled = false;
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // 색상 초기화
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

            // 배경 이미지 처리
            if (!string.IsNullOrEmpty(data.backgroundImageName))
            {
                Sprite backgroundImage = Resources.Load<Sprite>("Backgrounds/" + data.backgroundImageName); // Resources 폴더 내 "Backgrounds" 폴더에서 로드
                if (backgroundImage != null)
                {
                    blackBox.SetActive(true);
                    blackBox.GetComponent<Image>().color = Color.white;
                    blackBox.GetComponent<SpriteRenderer>().sprite = backgroundImage;
                    blackBox.GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    Debug.LogError("Background image not found: " + data.backgroundImageName);
                }
            }

            // 스탠딩 일러스트 처리
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

            blackBox.SetActive(true); // blackBox GameObject 활성화
            blackBox.GetComponent<Image>().color = Color.white; // Image 컴포넌트 색상을 흰색으로

            if (diedMessageBackground != null) // 할당된 스프라이트가 있는지 확인
            {
                blackBox.GetComponent<SpriteRenderer>().sprite = diedMessageBackground; // 할당된 스프라이트 사용
                blackBox.GetComponent<SpriteRenderer>().enabled = true; // SpriteRenderer 활성화
            }
            else
            {
                Debug.LogError("Died Message Background Sprite가 DialogueManager 인스펙터에 할당되지 않았습니다!");
                blackBox.GetComponent<SpriteRenderer>().enabled = false; // 스프라이트 없으면 비활성화
            }

            isBlackBoxActive = true; // blackBox가 활성화되었음을 표시 (Update 함수에서 사용됨)

            diedText.gameObject.SetActive(true); // diedText 활성화
            diedText.text = "";
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(new DialogueData { dialogue = message, fontSize = diedText.fontSize }, diedText));
        }
        else
        {
            Debug.LogError("diedText가 DialogueManager Inspector 창에 할당되지 않았습니다!");
        }
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
        if (isTyping) return;

        if (dialogueIndex < currentDialogues.Count)
        {
            string currentId = currentDialogues[dialogueIndex].id;

            // 모든 시퀀스 컨트롤러에 현재 대화 ID 전달
            foreach (var controller in sequenceControllers)
            {
                controller.CheckDialogueEnd(currentId);
                if (controller.cameraController != null && controller.cameraController.IsMoving)
                {
                    Debug.Log($"카메라 이동 중, 다음 대사 진행 보류 (Controller: {controller.gameObject.name})");
                    return; // 하나의 컨트롤러라도 카메라 이동 중이면 진행 중단
                }
            }
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
            blackBox.GetComponent<SpriteRenderer>().enabled = false; // 배경 이미지 숨김
            blackBox.GetComponent<Image>().color = originalBlackBoxColor; // 색상 원래대로
            isBlackBoxActive = false;
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
        if (Input.GetKeyDown(KeyCode.Return) && !isBlackBoxActive)
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
        blackBox.GetComponent<SpriteRenderer>().enabled = false; // 배경 이미지 숨김
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // 색상 원래대로
        isBlackBoxActive = false;
        blackBox.SetActive(false);
       
    }
}