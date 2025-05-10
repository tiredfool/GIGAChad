using UnityEngine;

public class ConveyorBeltPhysics : MonoBehaviour
{
    [Tooltip("�о�� ���� ����")]
    public float pushForce = 15f;

    [Tooltip("�о�� ���� (Ʈ���� ������Ʈ ���� ���� X�� ���� * directionMultiplier). -1: ����, 1: ������")]
    public float directionMultiplier = -1f;

    private bool isConveying = false; // �����̾� ��Ʈ �۵� ���¸� �����ϴ� ����
    private Collider2D conveyorCollider; // �����̾� ��Ʈ�� Collider2D ������Ʈ

    void Start()
    {
        // ���� �ÿ��� �����̾� ��Ʈ�� ���� ���·� �ʱ�ȭ
        isConveying = false;
        conveyorCollider = GetComponent<Collider2D>();
        if (conveyorCollider == null || !conveyorCollider.isTrigger)
        {
            Debug.LogError("ConveyorBeltPhysics ��ũ��Ʈ�� ������ ������Ʈ�� Ʈ���� Collider2D�� �����ϴ�!", gameObject);
            enabled = false; // ������Ʈ ��Ȱ��ȭ
        }
    }

    // ��ü�� Ʈ���� ���ο� �ӹ����� ���� ��� ȣ��� (2D ����)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isConveying)
        {
            Rigidbody2D rb2D = other.GetComponent<Rigidbody2D>();

            if (other.CompareTag("Player") && rb2D != null && rb2D.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 pushDirection = transform.right * directionMultiplier;
                rb2D.AddForce(pushDirection * pushForce, ForceMode2D.Force);
            }
        }
    }

    // �ܺο��� �����̾� ��Ʈ�� �۵���Ű�� �Լ�
    public void StartConveying()
    {
        isConveying = true;
        if (conveyorCollider != null)
        {
            conveyorCollider.enabled = true; // Collider Ȱ��ȭ (Ȥ�� ��Ȱ��ȭ�Ǿ� ���� ��츦 ���)
        }
        Debug.Log(gameObject.name + ": �����̾� ��Ʈ �۵� ����");
    }

    // �ܺο��� �����̾� ��Ʈ�� ���ߴ� �Լ�
    public void StopConveying()
    {
        isConveying = false;
        Debug.Log(gameObject.name + ": �����̾� ��Ʈ �۵� �ߴ�");
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