using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogueTriggerHidden1 : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string targetTag = "PlayerHidden";
    public string startDialogueId;         // 시작 대사 ID
    public string endDialogueId;           // 종료 대사 ID
    private bool dialogueStarted = false;
    public DialogueSequenceController dialogueSequencer = null;
    public bool distroy = false;
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
                TriggerDialogueAfterDelay();
                Debug.Log("일반 대화 시작");
                DialogueManager.instance.StartDialogueByIdRange(startDialogueId, endDialogueId);
            }
            dialogueStarted = true;
            if (distroy) Destroy(gameObject);            //트리거 제거 (원하시면 이 줄은 주석 처리)
        }
    }
    private IEnumerator TriggerDialogueAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        // 여기서 다음 작업
    }

}