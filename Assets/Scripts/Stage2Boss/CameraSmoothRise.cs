using UnityEngine;

public class CameraSmoothRise : MonoBehaviour
{
    public float riseSpeed = 2f; // �ʴ� ��� �ӵ�
    public bool startRising = false; // ��� ���� ���� (PlatformTrigger���� true�� ����)

    void Update()
    {
        if (startRising)
        {
            transform.position += new Vector3(0f, riseSpeed * Time.deltaTime, 0f);
        }
    }

    // �ܺο��� ���� Ʈ���Ÿ� �ɱ� ���� �Լ�
    public void StartRising()
    {
        if (!startRising)
        {
            startRising = true;
        }
    }
}
