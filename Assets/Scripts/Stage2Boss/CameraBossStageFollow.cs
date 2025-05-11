using UnityEngine;

public class CameraBossStageFollow : MonoBehaviour
{
    public Transform target; // ���� ��� (��: ����)
    public float yOffset = 2f; // �������� ���� ��ġ�ϵ��� �ϴ� ������
    public float smoothSpeed = 2f; // ���󰡴� �ӵ�

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPos = transform.position;

            // Y�ุ ���󰡵�, ������ ����
            float targetY = target.position.y + yOffset;

            // ���� ī�޶󺸴� ������ ���� ���� ���� ���󰡵��� ���� (���� ��� ����)
            if (targetY > transform.position.y)
            {
                newPos.y = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
                transform.position = newPos;
            }
        }
    }
}
