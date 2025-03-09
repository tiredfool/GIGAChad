using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public DialogueManager dialogueManager; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueManager.StartDialogue();
        }
    }
}