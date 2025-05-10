using UnityEngine;

public class ConveyorBeltPhysics : MonoBehaviour
{
    [Tooltip("밀어내는 힘의 세기")]
    public float pushForce = 15f;

    [Tooltip("밀어내는 방향 (트리거 오브젝트 기준 로컬 X축 방향 * directionMultiplier). -1: 왼쪽, 1: 오른쪽")]
    public float directionMultiplier = -1f;

    private bool isConveying = false; // 컨베이어 벨트 작동 상태를 관리하는 변수
    private Collider2D conveyorCollider; // 컨베이어 벨트의 Collider2D 컴포넌트

    void Start()
    {
        // 시작 시에는 컨베이어 벨트를 멈춘 상태로 초기화
        isConveying = false;
        conveyorCollider = GetComponent<Collider2D>();
        if (conveyorCollider == null || !conveyorCollider.isTrigger)
        {
            Debug.LogError("ConveyorBeltPhysics 스크립트가 부착된 오브젝트에 트리거 Collider2D가 없습니다!", gameObject);
            enabled = false; // 컴포넌트 비활성화
        }
    }

    // 물체가 트리거 내부에 머무르는 동안 계속 호출됨 (2D 물리)
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

    // 외부에서 컨베이어 벨트를 작동시키는 함수
    public void StartConveying()
    {
        isConveying = true;
        if (conveyorCollider != null)
        {
            conveyorCollider.enabled = true; // Collider 활성화 (혹시 비활성화되어 있을 경우를 대비)
        }
        Debug.Log(gameObject.name + ": 컨베이어 벨트 작동 시작");
    }

    // 외부에서 컨베이어 벨트를 멈추는 함수
    public void StopConveying()
    {
        isConveying = false;
        Debug.Log(gameObject.name + ": 컨베이어 벨트 작동 중단");
    }

    // (선택 사항) 기즈모를 사용하여 에디터에서 방향을 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; // 밀어내는 방향 시각화
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