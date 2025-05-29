using UnityEngine;

public class CameraBossStageFollow : MonoBehaviour
{
    public Camera camera; // ī�޶� ����
    public Transform target; // ���� ��� (��: ����)
    public float yOffset = 2f; // �������� ���� ��ġ�ϵ��� �ϴ� ������
    public float smoothSpeed = 2f; // ���󰡴� �ӵ�

    public float targetOrthoSize = 30f; // ���� ũ�� 30���� ����
    public float targetAspectRatio = 10f / 30f; // ���� 10, ���� 30 ����

    void Start()
    {
        // ī�޶��� ���� ũ�� ����
        camera.orthographicSize = targetOrthoSize;

        // ī�޶��� ���� ũ�� ���
        float targetWidth = targetOrthoSize * 2 * targetAspectRatio;
        camera.aspect = targetWidth / (targetOrthoSize * 2);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Y�ุ ���󰡵�, ������ ����
            float targetY = target.position.y + yOffset;

            // ���� ī�޶󺸴� ������ ���� ���� ���� ���󰡵��� ���� (���� ��� ����)
            if (targetY > transform.position.y)
            {
                // ī�޶� ũ�� (����)�� ����
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetOrthoSize, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
