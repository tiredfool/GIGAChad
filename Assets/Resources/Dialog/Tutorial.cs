using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
   
    public KeyCode leftKey = KeyCode.W; // 기본값: 왼쪽 화살표 키
    public KeyCode rightKey = KeyCode.D; // 기본값: 오른쪽 화살표 키
    public KeyCode additionalKey = KeyCode.Space; // 기본값: 스페이스바

   
    bool isLeftKeyPressed = false;
    bool isRightKeyPressed = false;
    bool isAdditionalKeyPressed = false;
    bool action1 = false;
    bool action2 = false;
    public float delayForDialogue1 = 0.5f;
    public float delayForDialogue2 = 2f;
    private bool action1Executed = false; // "T-2s" 대화가 시작되었는지
    private bool action2Executed = false; // "T-3s" 대화가 시작되었는지
    private bool isWaitingForDialogue1 = false;
    private bool isWaitingForDialogue2 = false;
    // 대기 시간을 위한 타이머 변수
    private float currentDelayTimer = 0f;
    private bool isDelaying = false; // 현재 대기 중인지
    void Update()
    {
        CheckAllThreeKeysPressed();
    }

    void CheckAllThreeKeysPressed()
    {
        // 1. 좌 키, 우 키, 추가 키(스페이스바)가 모두 눌려있는지 확인
        if (!isLeftKeyPressed) isLeftKeyPressed = Input.GetKey(leftKey);
        if (!isRightKeyPressed) isRightKeyPressed = Input.GetKey(rightKey);
        if (!isAdditionalKeyPressed && action1) isAdditionalKeyPressed = Input.GetKey(additionalKey);

        if (isLeftKeyPressed && isRightKeyPressed && !action1)
        {
            //0.5초대기
            StartCoroutine(StartDialogue1AfterDelay());
           // DialogueManager.instance.StartDialogueByIdRange("T-2s","T-2e");

            action1 = true;
        }
        if (isAdditionalKeyPressed && action1 &&!action2)
        {
            //0.5초대기
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
        Debug.Log($"<color=blue>첫 번째 대화 시작 전 {delayForDialogue1}초 대기 중...</color>");
        isDelaying = true; // 대기 시작
        currentDelayTimer = delayForDialogue1; // 타이머 설정
        yield return new WaitForSeconds(delayForDialogue1); // 실제 대기
        isDelaying = false; // 대기 종료
        Debug.Log("<color=green>W+D 조합 감지 및 대기 종료. 'T-2s' 대화 시작!</color>");
        DialogueManager.instance.StartDialogueByIdRange("T-2s", "T-2e");
          
       
    }

    // Space 키 감지 후 대기 및 두 번째 대화 시작 코루틴
    private IEnumerator StartDialogue2AfterDelay()
    {
        Debug.Log("<color=purple>Space 조합 감지 및 대기 종료. 'T-3s' 대화 시작!</color>");
        isDelaying = true; // 대기 시작
        currentDelayTimer = delayForDialogue2; // 타이머 설정
        yield return new WaitForSeconds(delayForDialogue2); // 실제 대기
        isDelaying = false; // 대기 종료
        Debug.Log("<color=purple>W+D+Space 조합 감지 및 대기 종료. 'T-3s' 대화 시작!</color>");
        DialogueManager.instance.StartDialogueByIdRange("T-3s", "T-3e");
        action2 = true;
    }
      
    }

