using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager; 
    public string targetTag = "Player";   
    public string startDialogueId;        // 시작 대사 ID
    public string endDialogueId;          // 종료 대사 ID
    private bool dialogueStarted = false; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!dialogueStarted && other.CompareTag(targetTag)) 
        {
            Debug.Log("대화 시작");
            List<DialogueData> sceneDialogues = GetDialoguesForScene(startDialogueId, endDialogueId); 
            dialogueManager.SetDialogues(sceneDialogues);   
            dialogueManager.StartDialogue();                
            dialogueStarted = true;                        
            //Destroy(gameObject);         //트리거 제거                   
        }
    }
    List<DialogueData> GetDialoguesForScene(string startId, string endId) // id기반으로 해당 씬에서 출력한 대사 리스트 제작
    {
        List<DialogueData> sceneDialogues = new List<DialogueData>();
        bool inRange = false; 

        List<DialogueData> allDialogues = dialogueManager.GetDialogues();

        foreach (DialogueData dialogue in allDialogues)
        {
            Debug.Log($"Checking dialogue {dialogue.id}");
            if (dialogue.id == startId)
            {
                inRange = true; 
                Debug.Log($"Found start dialogue {dialogue.id}");
            }

            if (inRange)
            {
                sceneDialogues.Add(dialogue); 
                Debug.Log($"Adding dialogue {dialogue.id} to sceneDialogues");
            }

            if (dialogue.id == endId)
            {
                Debug.Log($"Found end dialogue {dialogue.id}");
                break; 
            }
        }

        return sceneDialogues;
    }
}