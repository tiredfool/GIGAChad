using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
   
    public KeyCode leftKey1 = KeyCode.A; // �⺻��: ���� ȭ��ǥ Ű
    public KeyCode rightKey1 = KeyCode.D; // �⺻��: ������ ȭ��ǥ Ű
    public KeyCode leftKey2 = KeyCode.LeftArrow; // �⺻��: ���� ȭ��ǥ Ű
    public KeyCode rightKey2 = KeyCode.RightArrow; // �⺻��: ������ ȭ��ǥ Ű
    public KeyCode additionalKey = KeyCode.Space; // �⺻��: �����̽���

   
    bool isLeftKeyPressed = false;
    bool isRightKeyPressed = false;
    bool isAdditionalKeyPressed = false;
    bool action1 = false;
    bool action2 = false;
    public float delayForDialogue1 = 0.5f;
    public float delayForDialogue2 = 2f;
    private bool action1Executed = false; // "T-2s" ��ȭ�� ���۵Ǿ�����
    private bool action2Executed = false; // "T-3s" ��ȭ�� ���۵Ǿ�����
    private bool isWaitingForDialogue1 = false;
    private bool isWaitingForDialogue2 = false;
    // ��� �ð��� ���� Ÿ�̸� ����
    private float currentDelayTimer = 0f;
    private bool isDelaying = false; // ���� ��� ������
    void Update()
    {
        CheckAllThreeKeysPressed();
    }

    void CheckAllThreeKeysPressed()
    {
      
        if (!isLeftKeyPressed) isLeftKeyPressed = Input.GetKey(leftKey1);
        if (!isRightKeyPressed) isRightKeyPressed = Input.GetKey(rightKey1);
        if (!isLeftKeyPressed) isLeftKeyPressed = Input.GetKey(leftKey2);
        if (!isRightKeyPressed) isRightKeyPressed = Input.GetKey(rightKey2);
        if (!isAdditionalKeyPressed && action1) isAdditionalKeyPressed = Input.GetKey(additionalKey);

        if (isLeftKeyPressed && isRightKeyPressed && !action1)
        {
            //0.5�ʴ��
            StartCoroutine(StartDialogue1AfterDelay());
           // DialogueManager.instance.StartDialogueByIdRange("T-2s","T-2e");

            action1 = true;
        }
        if (isAdditionalKeyPressed && action1 &&!action2)
        {
            //0.5�ʴ��
            StartCoroutine(StartDialogue2AfterDelay());
            // DialogueManager.instance.StartDialogueByIdRange("T-3s", "T-3e");
           
        }
        if (action1 && action2)
        {
            Destroy(this.gameObject);
        }


    }

    private IEnumerator StartDialogue1AfterDelay()
    {
        Debug.Log($"<color=blue>ù ��° ��ȭ ���� �� {delayForDialogue1}�� ��� ��...</color>");
        isDelaying = true; // ��� ����
        currentDelayTimer = delayForDialogue1; // Ÿ�̸� ����
        yield return new WaitForSeconds(delayForDialogue1); // ���� ���
        isDelaying = false; // ��� ����
        Debug.Log("<color=green>W+D ���� ���� �� ��� ����. 'T-2s' ��ȭ ����!</color>");
        DialogueManager.instance.StartDialogueByIdRange("T-2s", "T-2e");
          
       
    }

    // Space Ű ���� �� ��� �� �� ��° ��ȭ ���� �ڷ�ƾ
    private IEnumerator StartDialogue2AfterDelay()
    {
        Debug.Log("<color=purple>Space ���� ���� �� ��� ����. 'T-3s' ��ȭ ����!</color>");
        isDelaying = true; // ��� ����
        currentDelayTimer = delayForDialogue2; // Ÿ�̸� ����
        yield return new WaitForSeconds(delayForDialogue2); // ���� ���
        isDelaying = false; // ��� ����
        Debug.Log("<color=purple>W+D+Space ���� ���� �� ��� ����. 'T-3s' ��ȭ ����!</color>");
        DialogueManager.instance.StartDialogueByIdRange("T-3s", "T-3e");
        action2 = true;
    }
      
    }

