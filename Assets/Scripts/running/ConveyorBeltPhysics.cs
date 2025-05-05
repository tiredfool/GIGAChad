using UnityEngine;

public class ConveyorBeltPhysics: MonoBehaviour
{
    [Tooltip("�о�� ���� ����")]
    public float pushForce = 15f; // �������� ���� ���� ������ �����غ� �� �ֽ��ϴ� (���� ���)

    [Tooltip("�о�� ���� (Ʈ���� ������Ʈ ���� ���� X�� ���� * directionMultiplier). -1: ����, 1: ������")]
    public float directionMultiplier = -1f; // �⺻��: ���� (-X ����)

    // ��ü�� Ʈ���� ���ο� �ӹ����� ���� ��� ȣ��� (2D ����)
    private void OnTriggerStay2D(Collider2D other)
    {
        // � ������Ʈ�� ���� ������ Ȯ��
       // Debug.Log("Trigger Stay with: " + other.gameObject.name);

        Rigidbody2D rb2D = other.GetComponent<Rigidbody2D>();

        if (other.CompareTag("Player") && rb2D != null && rb2D.bodyType == RigidbodyType2D.Dynamic) // �÷��̾� �±� Ȯ�� �߰�
        {
            // ���� �����ϱ� ������ �α� ���
           
            Vector2 pushDirection = transform.right * directionMultiplier;
            rb2D.AddForce(pushDirection * pushForce, ForceMode2D.Force);
        }
     
    }

    // (���� ����) ����� ����Ͽ� �����Ϳ��� ������ �ð������� ǥ��
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; // �о�� ���� �ð�ȭ
        Collider2D col = GetComponent<Collider2D>();
        Vector3 center = transform.position;
        if (col != null)
        {
            center = col.bounds.center;
        }

        Vector3 worldDirection = transform.right * directionMultiplier;
        Gizmos.DrawLine(center, center + worldDirection * 1.5f);
        Gizmos.DrawSphere(center + worldDirection * 1.5f, 0.1f);
    }
}