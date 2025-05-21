using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera; // Player를 따라다니는 Virtual Camera
    public CinemachineVirtualCamera targetCamera; // 특정 오브젝트를 바라보는 Virtual Camera
    public float transitionDuration = 1f;        // 카메라 전환 시간

    private bool isMoving = false;
    public bool IsMoving => isMoving; // 외부에서 카메라 이동 상태를 알 수 있도록 속성 추가

    // 🔴 현재 실행 중인 카메라 전환 코루틴을 저장할 변수
    private Coroutine cameraTransitionCoroutine;

    void Start()
    {
        // 시작 시 타겟 카메라 비활성화
        if (targetCamera != null)
        {
            targetCamera.gameObject.SetActive(false);
            Debug.Log($"{targetCamera.gameObject.name} 시작 시 비활성화");
        }
        else
        {
            Debug.LogError("Target Camera가 할당되지 않았습니다.");
        }
    }

    // 외부에서 카메라 전환을 시작하는 메서드
    public void StartCameraTransition()
    {
        // 🔴 이미 코루틴이 실행 중이라면 기존 코루틴을 중지하고 새로 시작합니다.
        // 또는 그냥 return하여 중복 실행을 막을 수 있습니다.
        // 여기서는 "작동을 안 해" 문제 해결을 위해 이전 코루틴을 강제로 중지하는 방식을 택합니다.
        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
            Debug.LogWarning("기존 카메라 전환 코루틴이 강제로 중지되었습니다.");
            isMoving = false; // 강제 중지 시 isMoving 플래그도 초기화
        }
        cameraTransitionCoroutine = StartCoroutine(MoveToTargetAndBackCoroutine());
    }


    private IEnumerator MoveToTargetAndBackCoroutine() // 메서드 이름 변경 (StartCameraTransition에서 호출되도록)
    {
        // 🔴 try-finally 블록을 사용하여 isMoving 플래그가 항상 false로 설정되도록 보장합니다.
        // 중간에 에러가 발생해도 finally 블록은 항상 실행됩니다.
        try
        {
            if (isMoving) // StartCameraTransition에서 StopCoroutine으로 처리했으므로, 여기서는 불필요하지만 안전을 위해 유지
            {
                Debug.LogWarning("MoveToTargetAndBackCoroutine: 이미 이동 중입니다. 중복 실행 방지.");
                yield break;
            }

            isMoving = true;
            Debug.Log("카메라 이동 시작");

            // Target 카메라 활성화
            if (targetCamera != null)
            {
                targetCamera.gameObject.SetActive(true);
                // 🔴 카메라 우선순위 설정 (전환 시작)
                playerCamera.Priority = 10;
                targetCamera.Priority = 20; // 타겟 카메라가 높은 우선순위를 가져 전환 시작
            }
            else
            {
                Debug.LogError("Target Camera가 null입니다.");
                yield break; // 에러 발생 시 코루틴 종료
            }

            // 🔴 Cinemachine Brain이 전환을 완료할 때까지 기다리는 방법:
            // CinemachineCore.Instance.Get=(0).Is ;를 사용하거나,
            // 더 간단하게는 전환 시간만큼 기다립니다.
            // 이 시간은 Cinemachine Brain의 Default Blend 설정과 일치해야 합니다.
            yield return new WaitForSecondsRealtime(transitionDuration);
            Debug.Log("카메라 타겟 이동 완료, 잠시 대기 후 복귀 시작");

            // 🔴 전환이 완료된 후, 잠시 대기 (타겟 카메라 시점 유지 시간)
            yield return new WaitForSecondsRealtime(2f); // 잠시 대기 (조절 가능)

            // Player 카메라로 복귀
            if (playerCamera != null)
            {
                targetCamera.Priority = 10;
                playerCamera.Priority = 20; // 플레이어 카메라가 높은 우선순위를 가져 복귀 시작
            }
            else
            {
                Debug.LogError("Player Camera가 null입니다.");
                yield break; // 에러 발생 시 코루틴 종료
            }

            // 🔴 복귀 전환 완료 대기
            yield return new WaitForSecondsRealtime(transitionDuration);

            // 🔴 타겟 카메라 비활성화 (선택 사항)
            // 비활성화하면 나중에 다시 활성화될 때 약간의 딜레이가 있을 수 있습니다.
            // 하지만 리소스 관점에서는 비활성화하는 것이 좋습니다.
            if (targetCamera != null)
            {
                targetCamera.gameObject.SetActive(false);
            }
            Debug.Log("카메라 복귀 완료");
        }
        finally
        {
            // 🔴 코루틴이 어떻게 끝나든 (정상 종료든, 에러든, 강제 중지든)
            // isMoving 플래그는 항상 false로 설정되도록 보장합니다.
            isMoving = false;
            cameraTransitionCoroutine = null; // 코루틴 참조 해제
        }
    }
}