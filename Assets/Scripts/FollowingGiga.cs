using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // 플레이어 Transform
    public float distance = 2.0f; // 플레이어로부터의 수평 거리
    public float height = 1.0f; // 플레이어로부터의 수직 높이
    public float damping = 3.0f; // 카메라 이동의 부드러움 정도
    public float depthOffset = 0f; // Z축 (깊이) 오프셋 - 거의 사용되지 않음

    private bool isNegativeDistance = false; // 거리가 음수(좌측)인지 양수(우측)인지

    public float shakeAmount = 0.1f; // 흔들림 강도
    public float shakeSpeed = 10.0f; // (현재 코드에서는 사용되지 않음)
    private bool isShaking = false; // 흔들림 활성화 여부

    private SpriteRenderer spriteRenderer; // 이 스크립트가 붙어있는 GameObject의 SpriteRenderer

    void Awake() // Start 대신 Awake에서 초기화하는 것이 더 안전합니다.
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("FollowPlayer: 이 GameObject에 SpriteRenderer 컴포넌트를 찾을 수 없습니다. 좌우 반전 기능을 사용하려면 추가해야 합니다.", this);
        }
        // else { Debug.Log("FollowPlayer: SpriteRenderer 컴포넌트 발견 및 초기화 완료."); }
    }
    private void Start()
    {
       
    }
    void LateUpdate()
    {
        if (!player) // 플레이어 Transform이 할당되지 않았다면 함수 종료
        {
            // Debug.LogWarning("FollowPlayer: Player Transform이 할당되지 않았습니다!");
            return;
        }

        // 현재 거리를 계산 (음수면 플레이어 왼쪽, 양수면 플레이어 오른쪽)
        float currentDistance = isNegativeDistance ? -distance : distance;

        // 플레이어 위치를 기준으로 목표 위치 계산 (수평거리, 수직높이)
        Vector2 desiredPosition = (Vector2)player.position + new Vector2(-currentDistance, height);

        // 현재 위치에서 목표 위치로 부드럽게 이동
        Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, damping * Time.unscaledDeltaTime);

        // Z축은 고정하고 X, Y만 업데이트
        transform.position = new Vector3(smoothedPosition.x + depthOffset, smoothedPosition.y, transform.position.z);

        // 흔들림이 활성화된 경우 랜덤한 오프셋 추가
        if (isShaking)
        {
            // Random.insideUnitSphere는 3D 벡터를 반환하지만, 2D 게임에서는 X, Y만 사용됩니다.
            transform.position += Random.insideUnitSphere * shakeAmount;
        }
    }

    // 외부에서 거리를 특정 값으로 설정하고 스프라이트 반전
    public void SetNegativeDistance(bool value)
    {
        isNegativeDistance = value;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = value; // 스프라이트 좌우 반전
        }
    }

    // 현재 거리 상태를 토글하고 스프라이트 반전
    public void togleLocate()
    {
        isNegativeDistance = !isNegativeDistance; // 현재 상태를 반전

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isNegativeDistance; // 반전된 상태에 따라 스프라이트 좌우 반전
        }
        else
        {
            Debug.LogWarning("FollowPlayer: togleLocate 호출 시 SpriteRenderer가 null입니다. GameObject에 SpriteRenderer가 있는지 확인하세요.");
        }
    }

    // 흔들림 활성화/비활성화
    public void SetShake(bool value)
    {
        isShaking = value;
        if (!value) // 흔들림이 비활성화될 때
        {
            // 흔들림이 멈추면 오브젝트 위치를 플레이어 기준 이상적인 위치로 즉시 되돌림
            if (player != null)
            {
                float currentDistance = isNegativeDistance ? -distance : distance;
                Vector2 targetPosition = (Vector2)player.position + new Vector2(-currentDistance, height);
                transform.position = new Vector3(targetPosition.x + depthOffset, targetPosition.y, transform.position.z);
            }
        }
    }

    // GameObject 활성화/비활성화
    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

}