using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogueTriggerHidden1 : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string targetTag = "PlayerHidden";
    public string startDialogueId;         // ���� ��� ID
    public string endDialogueId;           // ���� ��� ID
    private bool dialogueStarted = false;
    public DialogueSequenceController dialogueSequencer = null;
    public bool distroy = false;
    void OnTriggerEnter2D(Collider2D other)
    {

        if (!dialogueStarted && other.CompareTag(targetTag))
        {
            if (dialogueSequencer != null)
            {
                Debug.Log("DialogueSequenceController�� ���� ��ȭ ����");
                dialogueSequencer.StartDialogueSequence(startDialogueId, endDialogueId);
            }
            else
            {
                TriggerDialogueAfterDelay();
                Debug.Log("�Ϲ� ��ȭ ����");
                DialogueManager.instance.StartDialogueByIdRange(startDialogueId, endDialogueId);
            }
            dialogueStarted = true;
            if (distroy) Destroy(gameObject);            //Ʈ���� ���� (���Ͻø� �� ���� �ּ� ó��)
        }
    }
    private IEnumerator TriggerDialogueAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        // ���⼭ ���� �۾�
    }

}