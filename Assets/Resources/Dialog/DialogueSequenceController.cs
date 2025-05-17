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
    public CameraController cameraController; // �� ��Ʈ�ѷ��� ����� CameraController
    public string cameraTriggerEndId = "";
    public DialogueSequence postCameraDialogue;
    private bool isCameraSequenceActive = false;
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = DialogueManager.instance;
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager �ν��Ͻ��� ã�� �� �����ϴ�.");
            enabled = false;
        }
        if (cameraController == null)
        {
            Debug.LogWarning($"{gameObject.name}: CameraController�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void StartDialogueSequence(string startId, string endId)
    {
        dialogueManager.StartDialogueByIdRange(startId, endId);
        isCameraSequenceActive = true;
    }

    // DialogueManager���� ���� ID�� �޾� Ʈ���� ���� Ȯ��
    public void CheckDialogueEnd(string currentDialogueId)
    {
        if (isCameraSequenceActive && !string.IsNullOrEmpty(cameraTriggerEndId) && currentDialogueId == cameraTriggerEndId && cameraController != null && !cameraController.IsMoving)
        {
            Debug.Log($"{gameObject.name}: ��ȭ ID '{cameraTriggerEndId}' ����, ī�޶� �̵� ����");
            StartCoroutine(HandleCameraMovement());
        }
    }

    private IEnumerator HandleCameraMovement()
    {
        dialogueManager.EndDialogue();
        yield return StartCoroutine(cameraController.MoveToTargetAndBack());

        // �߰�: ī�޶� ���� ������ ���
        while (cameraController != null && cameraController.IsMoving)
        {
            Debug.Log("ī�޶� �̵� ��... ��ȭ ���� ���");
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);
        Debug.Log("ī�޶� �̵� �Ϸ�, ���� ��ȭ ����");
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