using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera; // Player�� ����ٴϴ� Virtual Camera
    public CinemachineVirtualCamera targetCamera; // Ư�� ������Ʈ�� �ٶ󺸴� Virtual Camera
    public float transitionDuration = 1f;       // ī�޶� ��ȯ �ð�
    private bool isMoving = false;

    public bool IsMoving => isMoving; // �ܺο��� ī�޶� �̵� ���¸� �� �� �ֵ��� �Ӽ� �߰�

    public IEnumerator MoveToTargetAndBack()
    {
        if (isMoving) yield break; // �̹� �̵� ���̸� �ߺ� ���� ����
        isMoving = true;
        Debug.Log("ī�޶� �̵� ����");

        // Target ī�޶� Ȱ��ȭ
        targetCamera.Priority = 20;
        playerCamera.Priority = 10;

        yield return new WaitForSecondsRealtime(transitionDuration);

        Debug.Log("ī�޶� Ÿ�� �̵� �Ϸ�, ��� ��� �� ���� ����");
        yield return new WaitForSecondsRealtime(2f); // ��� ��� (���� ����)

        // Player ī�޶� ����
        playerCamera.Priority = 20;
        targetCamera.Priority = 10;

        yield return new WaitForSecondsRealtime(transitionDuration);

        isMoving = false;
        Debug.Log("ī�޶� ���� �Ϸ�");
    }
}