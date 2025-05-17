using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; 
using TMPro;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI diedText;
    public Image portraitImage;
    public GameObject dialogueBox;
    public GameObject blackBox; // ��� �̹��� ǥ�ÿ��� ���
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
    private Color originalBlackBoxColor; // blackBox�� �ʱ� ���� ����
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
        originalBlackBoxColor = blackBox.GetComponent<Image>().color; // �ʱ� ���� ����
        LoadDialogueFromJson();
        follower.SetVisible(false);
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);

        // blackBox�� SpriteRenderer ������Ʈ�� ������ �߰�
        if (blackBox.GetComponent<SpriteRenderer>() == null)
        {
            blackBox.AddComponent<SpriteRenderer>();
            blackBox.GetComponent<SpriteRenderer>().sortingOrder = -1; // ��ȭâ �ڿ� ǥ�õǵ��� ���� (���� ����)
        }
        blackBox.GetComponent<SpriteRenderer>().enabled = false; // �ʱ⿡�� ��Ȱ��ȭ
        blackBox.GetComponent<Image>().enabled = true; // Image ������Ʈ�� Ȱ��ȭ (���� �����)
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
        TextAsset textAsset = Resources.Load<TextAsset>(jsonFileName);
        if (textAsset == null)
        {
            Debug.LogError("JSON ���� '" + jsonFileName + "'�� ã�� �� �����ϴ�.");
            return;
        }

        string jsonString = textAsset.text;
        DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(jsonString);
        allDialogues = new List<DialogueData>(wrapper.dialogues);

        Debug.Log("Loaded " + allDialogues.Count + " dialogues from JSON.");
    }

    public void SetDialogues(List<DialogueData> dialogues)
    {
        currentDialogues = dialogues;
        dialogueIndex = 0;
    }

    public void StartDialogue()
    {
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
        ShowDialogue(currentDialogues[dialogueIndex]);
        dialogueStarted = true;
    }

    void ShowDialogue(DialogueData data)
    {
        // ��� ��ȭ �ڽ� �� ���� ��� ��Ȱ��ȭ (��� �̹��� ����)
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        blackBox.GetComponent<SpriteRenderer>().enabled = false;
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // ���� �ʱ�ȭ
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

            // ��� �̹��� ó��
            if (!string.IsNullOrEmpty(data.backgroundImageName))
            {
                Sprite backgroundImage = Resources.Load<Sprite>("Backgrounds/" + data.backgroundImageName); // Resources ���� �� "Backgrounds" �������� �ε�
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

            // ���ĵ� �Ϸ���Ʈ ó��
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
            StartCoroutine(WaitForBlackBoxEnd(data.blackBoxDuration));
        }
        else
        {
            Debug.LogError("Unknown dialogue type: " + data.dialogueType);
        }
    }

    IEnumerator WaitForBlackBoxEnd(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (isBlackBoxActive)
        {
            NextDialogue();
        }
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
    }

    public void SetDiedMessage(string message)
    {
        if (diedText != null)
        {
            diedText.text = "";
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(new DialogueData { dialogue = message, fontSize = diedText.fontSize }, diedText));
        }
        else
        {
            Debug.LogError("diedText�� DialogueManager Inspector â�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }
    IEnumerator ShakeScreen()
    {
        float elapsed = 0.0f;
        follower.SetShake(true);
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

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

        dialogueIndex++;
        if (dialogueIndex < currentDialogues.Count)
        {
            ShowDialogue(currentDialogues[dialogueIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        blackBox.GetComponent<SpriteRenderer>().enabled = false; // ��� �̹��� ����
        blackBox.GetComponent<Image>().color = originalBlackBoxColor; // ���� �������
        isBlackBoxActive = false;
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        playerController.SetTalking(false);
        Time.timeScale = 1f;
        follower.SetVisible(false);
        dialogueStarted = false;
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

    public void StartDialogueByIdRange(string startId, string endId)// �ܺο��� �ν��Ͻ��� �ҷ��� ��ȭ ���� ����
    {
        if (dialogueStarted) // �̹� ��ȭ�� ���� ���̸� �ߺ� ���� ����
        {
            Debug.LogWarning("�̹� ��ȭ�� ���� ���Դϴ�.");
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
            Debug.LogWarning($"ID '{startId}'���� '{endId}'������ ��ȭ�� ã�� �� �����ϴ�.");
        }
    }

    List<DialogueData> GetDialoguesForScene(string startId, string endId) 
    {
        List<DialogueData> sceneDialogues = new List<DialogueData>();
        bool inRange = false;

        List<DialogueData> allDialogues = GetDialogues(); 

        if (allDialogues == null || allDialogues.Count == 0)
        {
            Debug.LogError("DialogueManager���� ��ȭ �����͸� �ҷ����� ���߽��ϴ�.");
            return null;
        }

        foreach (DialogueData dialogue in allDialogues)
        {
            Debug.Log($"���� Ȯ�� ���� ��� ID: {dialogue.id}");
            if (dialogue.id == startId)
            {
                inRange = true;
                Debug.Log($"���� ��� ã�� (ID: {dialogue.id})");
            }

            if (inRange)
            {
                sceneDialogues.Add(dialogue);
                Debug.Log($"�� ��ȭ ��Ͽ� �߰� (ID: {dialogue.id})");
            }

            if (dialogue.id == endId)
            {
                Debug.Log($"���� ��� ã�� (ID: {dialogue.id}), ��ȭ ��� ���� �Ϸ�");
                break;
            }
        }

        return sceneDialogues;
    }

}