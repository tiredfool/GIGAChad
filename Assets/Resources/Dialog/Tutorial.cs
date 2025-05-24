using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    // VirtualInputManager에 등록된 가상 버튼 이름을 사용합니다.
    public string virtualLeftButton = "Left";
    public string virtualRightButton = "Right";
    public string virtualJumpButton = "Jump"; // Space 키에 매핑된 가상 버튼 이름

    // 각 가상 버튼이 "한 번이라도 눌렸는지" 여부를 추적하는 플래그
    private bool hasLeftBeenPressedOnce = false;
    private bool hasRightBeenPressedOnce = false;
    private bool hasJumpBeenPressedOnce = false; // 튜토리얼 2단계용

    // 각 튜토리얼 단계의 완료 여부
    private bool tutorialPhase1Completed = false; // 좌우 이동 감지 완료
    private bool tutorialPhase2Completed = false; // 점프 감지 완료

    public float delayForDialogue1 = 0.5f; // 좌우 이동 감지 후 첫 대화 시작까지 대기 시간
    public float delayForDialogue2 = 2f;   // 점프 감지 후 두 번째 대화 시작까지 대기 시간

    // 코루틴 중복 실행 방지를 위한 플래그
    private bool isDialogue1CoroutineRunning = false;
    private bool isDialogue2CoroutineRunning = false;

    void Start()
    {
        // DialogueManager.instance가 싱글톤으로 잘 설정되어 있다면 이 부분은 필요 없을 수 있습니다.
        // if (DialogueManager.instance == null) {
        //     Debug.LogError("DialogueManager.instance가 초기화되지 않았습니다. 튜토리얼 스크립트보다 먼저 초기화되는지 확인하세요.");
        // }
    }

    void Update()
    {
        // 튜토리얼 단계별로 진행
        if (!tutorialPhase1Completed)
        {
            CheckPhase1Input();
        }
        else if (!tutorialPhase2Completed)
        {
            CheckPhase2Input();
        }
        else // 모든 튜토리얼 단계 완료
        {
            Debug.Log("<color=green>모든 튜토리얼 단계 완료. 튜토리얼 스크립트 비활성화.</color>");
             Destroy(this.gameObject); // 더 이상 필요 없으므로 제거
            
        }
    }

    void CheckPhase1Input()
    {
        // 왼쪽 버튼이 방금 눌렸는지 확인하고, 한 번이라도 눌렸으면 플래그를 true로 설정
        if (!hasLeftBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualLeftButton))
        {
            hasLeftBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualLeftButton}' 가상 버튼 감지 (튜토리얼 1단계).</color>");
        }

        // 오른쪽 버튼이 방금 눌렸는지 확인하고, 한 번이라도 눌렸으면 플래그를 true로 설정
        if (!hasRightBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualRightButton))
        {
            hasRightBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualRightButton}' 가상 버튼 감지 (튜토리얼 1단계).</color>");
        }

        // 좌우 버튼이 모두 한 번씩 눌렸고, 아직 코루틴이 실행 중이 아니라면 대화 시작
        if (hasLeftBeenPressedOnce && hasRightBeenPressedOnce && !isDialogue1CoroutineRunning)
        {
            Debug.Log("<color=blue>좌우 가상 버튼 모두 감지 완료 (튜토리얼 1단계).</color>");
            StartCoroutine(StartDialogue1AfterDelay());
        }
    }

    void CheckPhase2Input()
    {
        // 점프 버튼이 방금 눌렸는지 확인하고, 한 번이라도 눌렸으면 플래그를 true로 설정
        if (!hasJumpBeenPressedOnce && VirtualInputManager.Instance.GetKeyOrButtonDown(virtualJumpButton))
        {
            hasJumpBeenPressedOnce = true;
            Debug.Log($"<color=blue>'{virtualJumpButton}' 가상 버튼 감지 (튜토리얼 2단계).</color>");
        }

        // 점프 버튼이 한 번이라도 눌렸고, 아직 코루틴이 실행 중이 아니라면 대화 시작
        if (hasJumpBeenPressedOnce && !isDialogue2CoroutineRunning)
        {
            Debug.Log("<color=blue>점프 가상 버튼 감지 완료 (튜토리얼 2단계).</color>");
            StartCoroutine(StartDialogue2AfterDelay());
        }
    }

    private IEnumerator StartDialogue1AfterDelay()
    {
        isDialogue1CoroutineRunning = true; // 코루틴 시작 플래그 설정
        Debug.Log($"<color=blue>첫 번째 대화 시작 전 {delayForDialogue1}초 대기 중...</color>");
        yield return new WaitForSeconds(delayForDialogue1); // 실제 대기

        Debug.Log("<color=green>좌우 가상 버튼 각각 감지 및 대기 종료. 'T-2s' 대화 시작!</color>");
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogueByIdRange("T-2s", "T-2e");
        }
        else
        {
            Debug.LogWarning("DialogueManager.instance가 null입니다. 대화를 시작할 수 없습니다.");
        }
        tutorialPhase1Completed = true; // 튜토리얼 1단계 완료
        isDialogue1CoroutineRunning = false; // 코루틴 종료 플래그 해제
    }

    private IEnumerator StartDialogue2AfterDelay()
    {
        isDialogue2CoroutineRunning = true; // 코루틴 시작 플래그 설정
        Debug.Log($"<color=purple>점프 가상 버튼 감지 후 {delayForDialogue2}초 대기 중...</color>");
        yield return new WaitForSeconds(delayForDialogue2); // 실제 대기

        Debug.Log("<color=purple>점프 가상 버튼 감지 및 대기 종료. 'T-3s' 대화 시작!</color>");
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogueByIdRange("T-3s", "T-3e");
        }
        else
        {
            Debug.LogWarning("DialogueManager.instance가 null입니다. 대화를 시작할 수 없습니다.");
        }
        tutorialPhase2Completed = true; // 튜토리얼 2단계 완료
        isDialogue2CoroutineRunning = false; // 코루틴 종료 플래그 해제
    }
}