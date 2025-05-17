using UnityEngine;
using System.Collections;

[System.Serializable]
public class DialogueSequence
{
    public string startId;
    public string endId;
}

public class DialogueSequenceController : MonoBehaviour
{
    public CameraController cameraController; // 이 컨트롤러가 사용할 CameraController
    public string cameraTriggerEndId = "";
    public DialogueSequence postCameraDialogue;
    private bool isCameraSequenceActive = false;
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = DialogueManager.instance;
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager 인스턴스를 찾을 수 없습니다.");
            enabled = false;
        }
        if (cameraController == null)
        {
            Debug.LogWarning($"{gameObject.name}: CameraController가 할당되지 않았습니다.");
        }
    }

    public void StartDialogueSequence(string startId, string endId)
    {
        dialogueManager.StartDialogueByIdRange(startId, endId);
        isCameraSequenceActive = true;
    }

    // DialogueManager에서 현재 ID를 받아 트리거 여부 확인
    public void CheckDialogueEnd(string currentDialogueId)
    {
        if (isCameraSequenceActive && !string.IsNullOrEmpty(cameraTriggerEndId) && currentDialogueId == cameraTriggerEndId && cameraController != null && !cameraController.IsMoving)
        {
            Debug.Log($"{gameObject.name}: 대화 ID '{cameraTriggerEndId}' 도달, 카메라 이동 시작");
            StartCoroutine(HandleCameraMovement());
        }
    }

    private IEnumerator HandleCameraMovement()
    {
        dialogueManager.EndDialogue();
        yield return StartCoroutine(cameraController.MoveToTargetAndBack());

        // 추가: 카메라가 멈출 때까지 대기
        while (cameraController != null && cameraController.IsMoving)
        {
            Debug.Log("카메라 이동 중... 대화 시작 대기");
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);
        Debug.Log("카메라 이동 완료, 다음 대화 시작");
        if (!string.IsNullOrEmpty(postCameraDialogue.startId) && !string.IsNullOrEmpty(postCameraDialogue.endId))
        {
            dialogueManager.StartDialogueByIdRange(postCameraDialogue.startId, postCameraDialogue.endId);
        }
        isCameraSequenceActive = false;
    }

    public void StartSpecificDialogue(string startId, string endId)
    {
        dialogueManager.StartDialogueByIdRange(startId, endId);
    }
}