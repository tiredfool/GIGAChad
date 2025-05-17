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
    public string standingImage; // 보여줄 스탠딩 이미지 이름
    public string standingPosition; // 스탠딩 이미지 위치 (left, right)
    public float blackBoxDuration = 10f;
    public string backgroundImageName; //배경이미지
}

public class DialogueWrapper
{
    public DialogueData[] dialogues;
}