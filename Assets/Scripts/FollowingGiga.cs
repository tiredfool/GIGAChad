using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance = 2.0f;
    public float height = 1.0f;
    public float damping = 3.0f;
    public float depthOffset = 0f;
    private bool isNegativeDistance = false; // 거리 음수화
    //private Vector3 initialPosition; // 초기 위치

    public float shakeAmount = 0.1f; // 흔들림 강도
    public float shakeSpeed = 10.0f; // 흔들림 속도
    private bool isShaking = false; // 흔들림 여부

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer 컴포넌트를 찾을 수 없습니다.");
        }
    }

        void LateUpdate()
    {
        if (!player) return;

        float currentDistance = isNegativeDistance ? -distance : distance; // 음수 거리 적용

        Vector2 desiredPosition = player.position + new Vector3(-currentDistance, height, 0f);
        Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, damping * Time.deltaTime);

        transform.position = new Vector3(smoothedPosition.x + depthOffset, smoothedPosition.y, transform.position.z);
        if (isShaking)
        {
            // 흔들림 효과 적용
            transform.position = transform.position + Random.insideUnitSphere * shakeAmount;
        }
    }

    // 외부에서 호출하여 거리를 음수로 변경하는 함수
    public void SetNegativeDistance(bool value)
    {
        isNegativeDistance = value;

        // SpriteRenderer가 있을 때만 좌우 반전
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = value; 
        }
    }

    public void SetShake(bool value) // 흔들림
    {
        isShaking = value;

      
    }

    public void SetVisible(bool value) // 투명화
    {
        gameObject.SetActive(value);
    }
}