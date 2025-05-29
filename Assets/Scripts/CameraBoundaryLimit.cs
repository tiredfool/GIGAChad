using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using Cinemachine;

public class CameraBoundaryLimit : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public Collider2D mapBounds; // 맵 전체를 감싸는 콜라이더
    public float cameraHorizontalOffset = 5f; // 카메라 좌우 경계 오프셋
    public float cameraVerticalOffset = 3f;   // 카메라 상하 경계 오프셋

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
            Debug.LogError("Cinemachine Virtual Camera가 할당되지 않았습니다!", this);
            enabled = false;
        }

        if (mapBounds == null)
        {
            Debug.LogError("맵 경계 콜라이더가 할당되지 않았습니다!", this);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (vcam != null && mapBounds != null && mainCamera != null && framingTransposer != null)
        {
            Bounds bounds = mapBounds.bounds;

            // 현재 카메라의 왼쪽, 오른쪽, 아래쪽, 위쪽 월드 좌표 계산
            float cameraLeftWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, vcam.transform.position.z - mainCamera.transform.position.z)).x - cameraHorizontalOffset;
            float cameraRightWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, vcam.transform.position.z - mainCamera.transform.position.z)).x + cameraHorizontalOffset;
            float cameraBottomWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, vcam.transform.position.z - mainCamera.transform.position.z)).y - cameraVerticalOffset;
            float cameraTopWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, vcam.transform.position.z - mainCamera.transform.position.z)).y + cameraVerticalOffset;



            // 맵 경계 콜라이더의 좌표
            float boundaryLeftWorldPos = bounds.min.x;
            float boundaryRightWorldPos = bounds.max.x;
            float boundaryBottomWorldPos = bounds.min.y;
            float boundaryTopWorldPos = bounds.max.y;
            Debug.Log($"Camera Left: {cameraLeftWorldPos}, Boundary Left: {boundaryLeftWorldPos}");
            Debug.Log($"Camera Right: {cameraRightWorldPos}, Boundary Right: {boundaryRightWorldPos}");

            Vector3 currentPos = vcam.transform.position;
            float newPosX = currentPos.x;
            float newPosY = currentPos.y;

            // 왼쪽 경계 제한
            if (cameraLeftWorldPos <= boundaryLeftWorldPos)
            {
                newPosX = boundaryLeftWorldPos + cameraHorizontalOffset;
                framingTransposer.m_XDamping = 0f;
            }
            // 오른쪽 경계 제한
            else if (cameraRightWorldPos >= boundaryRightWorldPos)
            {
                newPosX = boundaryRightWorldPos - cameraHorizontalOffset;
                framingTransposer.m_XDamping = 0f;
            }
            else
            {
                framingTransposer.m_XDamping = 1f; // 기본 댐핑 값
            }

            // 아래쪽 경계 제한
            if (cameraBottomWorldPos <= boundaryBottomWorldPos)
            {
                newPosY = boundaryBottomWorldPos + cameraVerticalOffset;
                framingTransposer.m_YDamping = 0f;
            }
            // 위쪽 경계 제한
            else if (cameraTopWorldPos >= boundaryTopWorldPos)
            {
                newPosY = boundaryTopWorldPos - cameraVerticalOffset;
                framingTransposer.m_YDamping = 0f;
            }
            else
            {
                framingTransposer.m_YDamping = 1f; // 기본 댐핑 값
            }

            vcam.transform.position = new Vector3(newPosX, newPosY, currentPos.z);
        }
    }
}