using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    // VirtualInputManager�� ��ϵ� ���� ��ư �̸��� ����մϴ�.
    public string virtualLeftButton = "Left";
    public string virtualRightButton = "Right";
    public string virtualJumpButton = "Jump"; // Space Ű�� ���ε� ���� ��ư �̸�

    // �� ���� ��ư�� "�� ���̶� ���ȴ���" ���θ� �����ϴ� �÷���
    private bool hasLeftBeenPressedOnce = false;
    private bool hasRightBeenPressedOnce = false;
    private bool hasJumpBeenPressedOnce = false; // Ʃ�丮�� 2�ܰ��

    // �� Ʃ�丮�� �ܰ��� �Ϸ� ����
    private bool tutorialPhase1Completed = false; // �¿� �̵� ���� �Ϸ�
    private bool tutorialPhase2Completed = false; // ���� ���� �Ϸ�

    public float delayForDialogue1 = 0.5f; // �¿� �̵� ���� �� ù ��ȭ ���۱��� ��� �ð�
    public float delayForDialogue2 = 2f;   // ���� ���� �� �� ��° ��ȭ ���۱��� ��� �ð�

    // �ڷ�ƾ �ߺ� ���� ������ ���� �÷���
    private bool isDialogue1CoroutineRunning = false;
    private bool isDialogue2CoroutineRunning = false;

    void Start()
    {
        // DialogueManager.instance�� �̱������� �� �����Ǿ� �ִٸ� �� �κ��� �ʿ� ���� �� �ֽ��ϴ�.
        // if (DialogueManager.instance == null) {
        //     Debug.LogError("DialogueManager.instance�� �ʱ�ȭ���� �ʾҽ��ϴ�. Ʃ�丮�� ��ũ��Ʈ���� ���� �ʱ�ȭ�Ǵ��� Ȯ���ϼ���.");
        // }
    }

    void Update()
    {
        // Ʃ�丮�� �ܰ躰�� ����
        if (!tutorialPhase1Completed)
        {
            CheckPhase1Input();
        }
        else if (!tutorialPhase2Completed)
        {
            CheckPhase2Input();
        }
        else // ��� Ʃ�丮�� �ܰ� �Ϸ�
        {
            Debug.Log("<color=green>��� Ʃ�丮�� �ܰ� �Ϸ�. Ʃ�丮�� ��ũ��Ʈ ��Ȱ��ȭ.</color>");
             Destroy(this.gameObject); // �� �̻� �ʿ� �����Ƿ� ����
            
        }
    }

    void CheckPhase1Input()
    {
        // ���� ��ư�� ��� ���ȴ��� Ȯ���ϰ�, �� ���̶� �������� �÷��׸� true�� ����
        if (!hasLeftBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualLeftButton))
        {
            hasLeftBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualLeftButton}' ���� ��ư ���� (Ʃ�丮�� 1�ܰ�).</color>");
        }

        // ������ ��ư�� ��� ���ȴ��� Ȯ���ϰ�, �� ���̶� �������� �÷��׸� true�� ����
        if (!hasRightBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualRightButton))
        {
            hasRightBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualRightButton}' ���� ��ư ���� (Ʃ�丮�� 1�ܰ�).</color>");
        }

        // �¿� ��ư�� ��� �� ���� ���Ȱ�, ���� �ڷ�ƾ�� ���� ���� �ƴ϶�� ��ȭ ����
        if (hasLeftBeenPressedOnce && hasRightBeenPressedOnce && !isDialogue1CoroutineRunning)
        {
            Debug.Log("<color=blue>�¿� ���� ��ư ��� ���� �Ϸ� (Ʃ�丮�� 1�ܰ�).</color>");
            StartCoroutine(StartDialogue1AfterDelay());
        }
    }

    void CheckPhase2Input()
    {
        // ���� ��ư�� ��� ���ȴ��� Ȯ���ϰ�, �� ���̶� �������� �÷��׸� true�� ����
        if (!hasJumpBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualJumpButton))
        {
            hasJumpBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualJumpButton}' ���� ��ư ���� (Ʃ�丮�� 2�ܰ�).</color>");
        }

        // ���� ��ư�� �� ���̶� ���Ȱ�, ���� �ڷ�ƾ�� ���� ���� �ƴ϶�� ��ȭ ����
        if (hasJumpBeenPressedOnce && !isDialogue2CoroutineRunning)
        {
            Debug.Log("<color=blue>���� ���� ��ư ���� �Ϸ� (Ʃ�丮�� 2�ܰ�).</color>");
            StartCoroutine(StartDialogue2AfterDelay());
        }
    }

    private IEnumerator StartDialogue1AfterDelay()
    {
        isDialogue1CoroutineRunning = true; // �ڷ�ƾ ���� �÷��� ����
        Debug.Log($"<color=blue>ù ��° ��ȭ ���� �� {delayForDialogue1}�� ��� ��...</color>");
        yield return new WaitForSeconds(delayForDialogue1); // ���� ���

        Debug.Log("<color=green>�¿� ���� ��ư ���� ���� �� ��� ����. 'T-2s' ��ȭ ����!</color>");
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogueByIdRange("T-2s", "T-2e");
        }
        else
        {
            Debug.LogWarning("DialogueManager.instance�� null�Դϴ�. ��ȭ�� ������ �� �����ϴ�.");
        }
        tutorialPhase1Completed = true; // Ʃ�丮�� 1�ܰ� �Ϸ�
        isDialogue1CoroutineRunning = false; // �ڷ�ƾ ���� �÷��� ����
    }

    private IEnumerator StartDialogue2AfterDelay()
    {
        isDialogue2CoroutineRunning = true; // �ڷ�ƾ ���� �÷��� ����
        Debug.Log($"<color=purple>���� ���� ��ư ���� �� {delayForDialogue2}�� ��� ��...</color>");
        yield return new WaitForSeconds(delayForDialogue2); // ���� ���

        Debug.Log("<color=purple>���� ���� ��ư ���� �� ��� ����. 'T-3s' ��ȭ ����!</color>");
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogueByIdRange("T-3s", "T-3e");
        }
        else
        {
            Debug.LogWarning("DialogueManager.instance�� null�Դϴ�. ��ȭ�� ������ �� �����ϴ�.");
        }
        tutorialPhase2Completed = true; // Ʃ�丮�� 2�ܰ� �Ϸ�
        isDialogue2CoroutineRunning = false; // �ڷ�ƾ ���� �÷��� ����
    }
}