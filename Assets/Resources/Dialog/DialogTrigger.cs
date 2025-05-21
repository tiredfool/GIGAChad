using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string targetTag = "Player";
    public string startDialogueId;         // 시작 대사 ID
    public string endDialogueId;           // 종료 대사 ID
    private bool dialogueStarted = false;
    public DialogueSequenceController dialogueSequencer = null;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!dialogueStarted && other.CompareTag(targetTag))
        {
            if (dialogueSequencer != null)
            {
                Debug.Log("DialogueSequenceController를 통해 대화 시작");
                dialogueSequencer.StartDialogueSequence(startDialogueId, endDialogueId);
            }
            else
            {
                Debug.Log("일반 대화 시작");
                DialogueManager.instance.StartDialogueByIdRange(startDialogueId, endDialogueId);
            }
            dialogueStarted = true;
            //Destroy(gameObject);            //트리거 제거 (원하시면 이 줄은 주석 처리)
        }
    }

  
}