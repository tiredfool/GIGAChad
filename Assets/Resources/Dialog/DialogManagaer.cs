using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List ����� ���� �߰�
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public GameObject dialogueBox;
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

    void Awake()
    {
        originalDialogueBoxPosition = dialogueBox.transform.localPosition; // ��ȭâ �ʱ� ��ġ ����
        dialogueBox.SetActive(false);
        LoadDialogueFromJson();
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

        dialogueIndex = 0;
        dialogueBox.SetActive(true);
        playerController.SetTalking(true); // ��ȭ ���� �� ������ ����
        ShowDialogue(currentDialogues[dialogueIndex]);
    }

    void ShowDialogue(DialogueData data)
    {
        nameText.text = data.npcName;

        Sprite portrait = portraitSprites.Find(s => s.name == data.portraitName);
        if (portrait != null)
        {
            portraitImage.sprite = portrait;
        }

        StopAllCoroutines();
        if (data.shakeScreen) StartCoroutine(ShakeScreen());
        StartCoroutine(TypeDialogue(data));
    }

    IEnumerator TypeDialogue(DialogueData data)
    {
        isTyping = true;
        dialogueText.fontSize = data.fontSize;
        dialogueText.text = "";

        foreach (char letter in data.dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator ShakeScreen() // ����
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            dialogueBox.transform.localPosition = originalDialogueBoxPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

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
        playerController.SetTalking(false);
        playerController.SetTalking(false);
    }

    public List<DialogueData> GetDialogues()
    {
        return allDialogues;
    }
    void Update() // ���� ���
    {

        if (Input.GetKeyDown(KeyCode.Return) )
        {
            NextDialogue();
        }
    }
}

// JSON �迭 �Ľ̿�...
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(List<T> array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(List<T> array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}