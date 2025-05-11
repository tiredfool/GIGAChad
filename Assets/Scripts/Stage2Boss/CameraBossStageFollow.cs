using UnityEngine;

public class CameraBossStageFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (예: 보스)
    public float yOffset = 2f; // 보스보다 위에 위치하도록 하는 오프셋
    public float smoothSpeed = 2f; // 따라가는 속도

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPos = transform.position;

            // Y축만 따라가되, 오프셋 적용
            float targetY = target.position.y + yOffset;

            // 현재 카메라보다 보스가 위에 있을 때만 따라가도록 제한 (무한 상승 느낌)
            if (targetY > transform.position.y)
            {
                newPos.y = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
                transform.position = newPos;
            }
        }
    }
}
