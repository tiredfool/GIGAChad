using UnityEngine;

public class ConveyorBeltPhysics: MonoBehaviour
{
    [Tooltip("밀어내는 힘의 세기")]
    public float pushForce = 15f; // 이전보다 값을 조금 높여서 시작해볼 수 있습니다 (마찰 고려)

    [Tooltip("밀어내는 방향 (트리거 오브젝트 기준 로컬 X축 방향 * directionMultiplier). -1: 왼쪽, 1: 오른쪽")]
    public float directionMultiplier = -1f; // 기본값: 왼쪽 (-X 방향)

    // 물체가 트리거 내부에 머무르는 동안 계속 호출됨 (2D 물리)
    private void OnTriggerStay2D(Collider2D other)
    {
        // 어떤 오브젝트와 접촉 중인지 확인
       // Debug.Log("Trigger Stay with: " + other.gameObject.name);

        Rigidbody2D rb2D = other.GetComponent<Rigidbody2D>();

        if (other.CompareTag("Player") && rb2D != null && rb2D.bodyType == RigidbodyType2D.Dynamic) // 플레이어 태그 확인 추가
        {
            // 힘을 적용하기 직전에 로그 출력
           
            Vector2 pushDirection = transform.right * directionMultiplier;
            rb2D.AddForce(pushDirection * pushForce, ForceMode2D.Force);
        }
     
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