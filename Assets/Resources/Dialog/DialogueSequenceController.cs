using UnityEngine;
using System.Collections;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가
using System.Collections.Generic; //

[System.Serializable]
public class DialogueSequence
{
    public string startId;
    public string endId;
}

// 특정 대화 ID에 연결될 이벤트를 위한 구조체
[System.Serializable]
public class DialogueActionEvent
{
    public string dialogueId; // 이 ID에 도달했을 때 이벤트를 발생시킵니다.
    public UnityEvent onDialogueIdReached; // 이 ID에 도달했을 때 호출될 UnityEvent
}

public class DialogueSequenceController : MonoBehaviour
{
    public CameraController cameraController; // 이 컨트롤러가 사용할 CameraController
    public string cameraTriggerEndId = "";
    public DialogueSequence postCameraDialogue;
    private bool isCameraSequenceActive = false;
   

    
    public DialogueActionEvent[] dialogueActions;
    private List<string> executedDialogueActionIds = new List<string>();

    void Start()
    {
      
    }

    public void StartDialogueSequence(string startId, string endId)
    {
        DialogueManager.instance.StartDialogueByIdRange(startId, endId);
        isCameraSequenceActive = true;
    }

    // DialogueManager에서 현재 ID를 받아 트리거 여부 확인
    public void CheckDialogueEnd(string currentDialogueId)
    {
        
            // 카메라 전환 로직 (기존 코드)
            if (isCameraSequenceActive && !string.IsNullOrEmpty(cameraTriggerEndId) && currentDialogueId == cameraTriggerEndId && cameraController != null && !cameraController.IsMoving)
            {
            Debug.Log($"{gameObject.name}: 대화 ID '{cameraTriggerEndId}' 도달, 카메라 이동 시작");
            cameraTriggerEndId = "NOT";
            StartCoroutine(HandleCameraMovement());
            }

        // 🚨 특정 대화 ID 도달 시 이벤트 실행 로직 추가
        foreach (var action in dialogueActions)
        {
            // ✨ 이미 실행된 액션인지 확인
            if (executedDialogueActionIds.Contains(action.dialogueId))
            {
                // 이미 실행된 액션이면 건너뜁니다.
                continue;
            }

            if (currentDialogueId == action.dialogueId)
            {
                Debug.Log($"대화 ID '{action.dialogueId}'에 도달하여 등록된 이벤트를 호출합니다.");
                action.onDialogueIdReached?.Invoke(); // 등록된 모든 함수 호출

                executedDialogueActionIds.Add(action.dialogueId);
              
            }
        }
    }

    private IEnumerator HandleCameraMovement()
    {

        DialogueManager.instance.EndDialogue();

        if (cameraController != null)
        {
            cameraController.StartCameraTransition();
        }

        while (cameraController != null && cameraController.IsMoving)
        {
            Debug.Log("카메라 이동 중... 대화 시작 대기");
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("카메라 이동 완료, 다음 대화 시작");
        if (!string.IsNullOrEmpty(postCameraDialogue.startId) && !string.IsNullOrEmpty(postCameraDialogue.endId))
        {
            StartSpecificDialogue(postCameraDialogue.startId, postCameraDialogue.endId);
        }
        isCameraSequenceActive = false;
    }

    public void StartSpecificDialogue(string startId, string endId)
    {
        DialogueManager.instance.StartDialogueByIdRange(startId, endId);
    }

    
}