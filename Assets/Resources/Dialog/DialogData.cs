using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string id = "basic";          // 대사 ID
    public string npcName;
    public string portraitName; 
    [TextArea(3, 10)]
    public string dialogue;
    public float fontSize = 24;
    public bool shakeScreen = false;
    public string dialogueType = "normal"; // 대화박스 타입을 위한 변수
}

public class DialogueWrapper
{
    public DialogueData[] dialogues;
}