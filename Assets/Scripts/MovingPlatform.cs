using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float amplitude = 1.0f; // �̵� �Ÿ�
    public float speed = 1.0f;     // �ӵ�

    private Vector3 startPos;
    private float timeOffset;

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2); // ���� ��ġ�� �����ϰ� ���� (�ε巯�� ����)
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed + timeOffset) + 1) / 2; // 0~1 ������ ����
        float smoothT = Mathf.SmoothStep(0, 1, t); // �ε巯�� ���� ȿ�� �߰�
        float newY = startPos.y + smoothT * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
