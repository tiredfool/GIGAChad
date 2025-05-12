using UnityEngine;

public class CameraSmoothRise : MonoBehaviour
{
    public float riseSpeed = 2f; // 초당 상승 속도
    public bool startRising = false; // 상승 시작 여부 (PlatformTrigger에서 true로 변경)

    void Update()
    {
        if (startRising)
        {
            transform.position += new Vector3(0f, riseSpeed * Time.deltaTime, 0f);
        }
    }

    // 외부에서 시작 트리거를 걸기 위한 함수
    public void StartRising()
    {
        if (!startRising)
        {
            startRising = true;
        }
    }
}
