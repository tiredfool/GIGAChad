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
    public Image portraitImage;
    public Image[] lifes;
    public GameObject dialogueBox;
    public GameObject blackBox; // �߰��� ���� ȸ��ڽ�
    public TextMeshProUGUI blackText;//�߰��� ���� ȸ��ڽ� �ؽ�Ʈ
    public string jsonFileName = "Dialog/dialogues"; // JSON ���� �̸�
    public float typingSpeed = 0.05f;
    public float fontSizeIncrease = 5f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    private int dialogueIndex = 0;
    private bool isTyping = false;

    private Vector3 originalDialogueBoxPosition; // ��ȭâ �ʱ� ��ġ

    private List<DialogueData> allDialogues = new List<DialogueData>();  // ��ü ���
    private List<DialogueData> currentDialogues = new List<DialogueData>(); // ���� �� ���
    public List<Sprite> portraitSprites = new List<Sprite>(); // �̹��� 
    public PlayerController playerController; // PlayerController ����
    public FollowPlayer follower; // ������� �Ⱑ����

    // ���ĵ� �Ϸ���Ʈ ���� ����
    public Image standingImageLeft;
    public Image standingImageRight;
    public List<Sprite> standingSprites = new List<Sprite>(); // ���ĵ� �Ϸ���Ʈ ��������Ʈ ����Ʈ

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� ���� (���� ����)
        }
        else
        {
            Destroy(gameObject);
        }

        originalDialogueBoxPosition = dialogueBox.transform.localPosition; // ��ȭâ �ʱ� ��ġ ����
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        LoadDialogueFromJson();
        follower.SetVisible(false); // �Ⱑ���� ��Ȱ��ȭ
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
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
   
    public void SetDialogues(List<DialogueData> dialogues) //Ʈ���ſ��� ��� �����ֱ�
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
        playerController.SetTalking(true); // ��ȭ ���� �� ������ ����
        Time.timeScale = 0f;
        ShowDialogue(currentDialogues[dialogueIndex]);
    }

    void ShowDialogue(DialogueData data)
    {
        // ��� ��ȭ �ڽ� �� ���ĵ� �Ϸ���Ʈ ��Ȱ��ȭ
        dialogueBox.SetActive(false);
        blackBox.SetActive(false);
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);

        if (data.dialogueType == "normal")
        {
            dialogueBox.SetActive(true);
            nameText.text = data.npcName;

            Sprite portrait = portraitSprites.Find(s => s.name == data.portraitName);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
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
            StopAllCoroutines();
            StartCoroutine(TypeDialogue(data, blackText));
        }
        else
        {
            Debug.LogError("Unknown dialogue type: " + data.dialogueType);
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
            yield return new WaitForSecondsRealtime(typingSpeed); // ������
        }

        isTyping = false;
    }

  
    IEnumerator ShakeScreen() // ����
    {
        float elapsed = 0.0f;
        follower.SetShake(true);
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            dialogueBox.transform.localPosition = originalDialogueBoxPosition + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime; // ������

            yield return null;
        }

        dialogueBox.transform.localPosition = originalDialogueBoxPosition;
    }

    public void NextDialogue()
    {
        if (isTyping) return; // Ÿ���� ���̸� �ѱ��� ����

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
        if (standingImageLeft != null) standingImageLeft.gameObject.SetActive(false);
        if (standingImageRight != null) standingImageRight.gameObject.SetActive(false);
        playerController.SetTalking(false);
        //  playerController.SetTalking(false);
        Time.timeScale = 1f;
        follower.SetShake(false);
        follower.SetVisible(false);
    }

    public List<DialogueData> GetDialogues()
    {
        return allDialogues;
    }

    void Update() // ���� ���
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NextDialogue();
        }
    }
}