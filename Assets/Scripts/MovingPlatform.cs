using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float amplitude = 1.0f; // 이동 거리
    public float speed = 1.0f;     // 속도

    private Vector3 startPos;
    private float timeOffset;

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2); // 시작 위치를 랜덤하게 설정 (부드러운 시작)
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed + timeOffset) + 1) / 2; // 0~1 범위로 조정
        float smoothT = Mathf.SmoothStep(0, 1, t); // 부드러운 감속 효과 추가
        float newY = startPos.y + smoothT * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
