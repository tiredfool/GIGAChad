using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera; // Player를 따라다니는 Virtual Camera
    public CinemachineVirtualCamera targetCamera; // 특정 오브젝트를 바라보는 Virtual Camera
    public float transitionDuration = 1f;       // 카메라 전환 시간
    private bool isMoving = false;

    public bool IsMoving => isMoving; // 외부에서 카메라 이동 상태를 알 수 있도록 속성 추가

    public IEnumerator MoveToTargetAndBack()
    {
        if (isMoving) yield break; // 이미 이동 중이면 중복 실행 방지
        isMoving = true;
        Debug.Log("카메라 이동 시작");

        // Target 카메라 활성화
        targetCamera.Priority = 20;
        playerCamera.Priority = 10;

        yield return new WaitForSecondsRealtime(transitionDuration);

        Debug.Log("카메라 타겟 이동 완료, 잠시 대기 후 복귀 시작");
        yield return new WaitForSecondsRealtime(2f); // 잠시 대기 (조절 가능)

        // Player 카메라 복귀
        playerCamera.Priority = 20;
        targetCamera.Priority = 10;

        yield return new WaitForSecondsRealtime(transitionDuration);

        isMoving = false;
        Debug.Log("카메라 복귀 완료");
    }
}