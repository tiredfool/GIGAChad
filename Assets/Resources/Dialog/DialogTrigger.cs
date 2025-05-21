using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string targetTag = "Player";
    public string startDialogueId;         // ���� ��� ID
    public string endDialogueId;           // ���� ��� ID
    private bool dialogueStarted = false;
    public DialogueSequenceController dialogueSequencer = null;

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
                Debug.Log("�Ϲ� ��ȭ ����");
                DialogueManager.instance.StartDialogueByIdRange(startDialogueId, endDialogueId);
            }
            dialogueStarted = true;
            //Destroy(gameObject);            //Ʈ���� ���� (���Ͻø� �� ���� �ּ� ó��)
        }
    }

  
}