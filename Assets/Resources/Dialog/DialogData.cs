using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string id = "basic";          // ด๋ป็ ID
    public string npcName;
    public string portraitName; 
    [TextArea(3, 10)]
    public string dialogue;
    public float fontSize = 24;
    public bool shakeScreen = false;
}

public class DialogueWrapper
{
    public DialogueData[] dialogues;
}