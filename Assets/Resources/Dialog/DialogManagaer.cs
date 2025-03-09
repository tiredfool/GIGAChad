using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public GameObject dialogueBox;
    public string jsonFileName = "Dialog/dialogues"; // JSON 파일 이름
    public float typingSpeed = 0.05f;
    public float fontSizeIncrease = 5f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    private int dialogueIndex = 0;
    private bool isTyping = false;

    private Vector3 originalDialogueBoxPosition; // 대화창 초기 위치

    private List<DialogueData> allDialogues = new List<DialogueData>();  // 전체 대사
    private List<DialogueData> currentDialogues = new List<DialogueData>(); // 현재 씬 대사
    public List<Sprite> portraitSprites = new List<Sprite>(); // 이미지 
    public PlayerController playerController; // PlayerController 참조

    void Awake()
    {
        originalDialogueBoxPosition = dialogueBox.transform.localPosition; // 대화창 초기 위치 저장
        dialogueBox.SetActive(false);
        LoadDialogueFromJson();
    }

    void LoadDialogueFromJson()
    {
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

    public void SetDialogues(List<DialogueData> dialogues) //트리거에서 대사 정해주기
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
        playerController.SetTalking(true); // 대화 시작 시 움직임 막기
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

    IEnumerator ShakeScreen() // 흔들기
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
        if (isTyping) return; // 타이핑 중이면 넘기지 않음

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
    void Update() // 다음 대사
    {

        if (Input.GetKeyDown(KeyCode.Return) )
        {
            NextDialogue();
        }
    }
}

// JSON 배열 파싱용...
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