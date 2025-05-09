using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using Cinemachine;

public class CameraBoundaryLimit : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public Collider2D mapBounds; // �� ��ü�� ���δ� �ݶ��̴�
    public float cameraHorizontalOffset = 5f; // ī�޶� �¿� ��� ������
    public float cameraVerticalOffset = 3f;   // ī�޶� ���� ��� ������

    private CinemachineFramingTransposer framingTransposer;
    private Camera mainCamera;

    void Start()
    {
        if (vcam != null)
        {
            framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            mainCamera = Camera.main;
        }
        else
        {
            Debug.LogError("Cinemachine Virtual Camera�� �Ҵ���� �ʾҽ��ϴ�!", this);
            enabled = false;
        }

        if (mapBounds == null)
        {
            Debug.LogError("�� ��� �ݶ��̴��� �Ҵ���� �ʾҽ��ϴ�!", this);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (vcam != null && mapBounds != null && mainCamera != null && framingTransposer != null)
        {
            Bounds bounds = mapBounds.bounds;

            // ���� ī�޶��� ����, ������, �Ʒ���, ���� ���� ��ǥ ���
            float cameraLeftWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, vcam.transform.position.z - mainCamera.transform.position.z)).x - cameraHorizontalOffset;
            float cameraRightWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, vcam.transform.position.z - mainCamera.transform.position.z)).x + cameraHorizontalOffset;
            float cameraBottomWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, vcam.transform.position.z - mainCamera.transform.position.z)).y - cameraVerticalOffset;
            float cameraTopWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, vcam.transform.position.z - mainCamera.transform.position.z)).y + cameraVerticalOffset;



            // �� ��� �ݶ��̴��� ��ǥ
            float boundaryLeftWorldPos = bounds.min.x;
            float boundaryRightWorldPos = bounds.max.x;
            float boundaryBottomWorldPos = bounds.min.y;
            float boundaryTopWorldPos = bounds.max.y;
            Debug.Log($"Camera Left: {cameraLeftWorldPos}, Boundary Left: {boundaryLeftWorldPos}");
            Debug.Log($"Camera Right: {cameraRightWorldPos}, Boundary Right: {boundaryRightWorldPos}");

            Vector3 currentPos = vcam.transform.position;
            float newPosX = currentPos.x;
            float newPosY = currentPos.y;

            // ���� ��� ����
            if (cameraLeftWorldPos <= boundaryLeftWorldPos)
            {
                newPosX = boundaryLeftWorldPos + cameraHorizontalOffset;
                framingTransposer.m_XDamping = 0f;
            }
            // ������ ��� ����
            else if (cameraRightWorldPos >= boundaryRightWorldPos)
            {
                newPosX = boundaryRightWorldPos - cameraHorizontalOffset;
                framingTransposer.m_XDamping = 0f;
            }
            else
            {
                framingTransposer.m_XDamping = 1f; // �⺻ ���� ��
            }

            // �Ʒ��� ��� ����
            if (cameraBottomWorldPos <= boundaryBottomWorldPos)
            {
                newPosY = boundaryBottomWorldPos + cameraVerticalOffset;
                framingTransposer.m_YDamping = 0f;
            }
            // ���� ��� ����
            else if (cameraTopWorldPos >= boundaryTopWorldPos)
            {
                newPosY = boundaryTopWorldPos - cameraVerticalOffset;
                framingTransposer.m_YDamping = 0f;
            }
            else
            {
                framingTransposer.m_YDamping = 1f; // �⺻ ���� ��
            }

            vcam.transform.position = new Vector3(newPosX, newPosY, currentPos.z);
        }
    }
}