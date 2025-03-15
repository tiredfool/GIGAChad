using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchMoveToRight : MonoBehaviour
{
    private SpriteRenderer switchRenderer; // ����ġ�� ��������Ʈ ������
    public Sprite greenLightSprite; // ����� �̹���
    public Sprite originalSprite; // ���� �̹��� ����
    public float moveDistance = 10f; // �̵� �Ÿ� (�������� 10ĭ)
    public float moveSpeed = 2f; // �̵� �ӵ�

    private Transform movePlatformTransform; // �̵��� ������ Transform
    private Vector3 targetPosition; // �̵� ��ǥ ����
    private bool isMoving = false; // �̵� �� ���� üũ

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite; // ���� �̹��� ����

        // "Move" �±׸� ���� ���� ã��
        GameObject movePlatform = GameObject.FindGameObjectWithTag("Move");
        if (movePlatform != null)
        {
            movePlatformTransform = movePlatform.transform;
            targetPosition = movePlatformTransform.position + new Vector3(moveDistance, 0, 0); // �������� 10ĭ �̵�
        }
        else
        {
            Debug.LogError("Move �±׸� ���� ������ ã�� �� �����ϴ�!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMoving)
        {
            Debug.Log("����ġ �۵�: ���� �̵� ����");
            switchRenderer.sprite = greenLightSprite; // ����ҷ� ����
            StartCoroutine(MovePlatform());
        }
    }

    IEnumerator MovePlatform()
    {
        isMoving = true;

        while (Vector3.Distance(movePlatformTransform.position, targetPosition) > 0.01f)
        {
            movePlatformTransform.position = Vector3.MoveTowards(movePlatformTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // �� ������ ���
        }

        movePlatformTransform.position = targetPosition; // ��Ȯ�� ��ġ ����
        isMoving = false;

        Debug.Log("���� �̵� �Ϸ�");
    }
}
