using UnityEngine;

public class CameraBossStageFollow : MonoBehaviour
{
    public Camera camera; // 카메라 참조
    public Transform target; // 따라갈 대상 (예: 보스)
    public float yOffset = 2f; // 보스보다 위에 위치하도록 하는 오프셋
    public float smoothSpeed = 2f; // 따라가는 속도

    public float targetOrthoSize = 30f; // 세로 크기 30으로 설정
    public float targetAspectRatio = 10f / 30f; // 가로 10, 세로 30 비율

    void Start()
    {
        // 카메라의 세로 크기 설정
        camera.orthographicSize = targetOrthoSize;

        // 카메라의 가로 크기 계산
        float targetWidth = targetOrthoSize * 2 * targetAspectRatio;
        camera.aspect = targetWidth / (targetOrthoSize * 2);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Y축만 따라가되, 오프셋 적용
            float targetY = target.position.y + yOffset;

            // 현재 카메라보다 보스가 위에 있을 때만 따라가도록 제한 (무한 상승 느낌)
            if (targetY > transform.position.y)
            {
                // 카메라 크기 (세로)만 조정
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetOrthoSize, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
