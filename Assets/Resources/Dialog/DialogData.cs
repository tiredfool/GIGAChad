using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string id = "basic";          // ��� ID
    public string npcName;
    public string portraitName; 
    [TextArea(3, 10)]
    public string dialogue;
    public float fontSize = 24;
    public bool shakeScreen = false;
    public string dialogueType = "normal"; // ��ȭ�ڽ� Ÿ���� ���� ����
    public string standingImage; // ������ ���ĵ� �̹��� �̸�
    public string standingPosition; // ���ĵ� �̹��� ��ġ (left, right)
    public float blackBoxDuration = 10f;
    public string backgroundImageName; //����̹���
}

public class DialogueWrapper
{
    public DialogueData[] dialogues;
}