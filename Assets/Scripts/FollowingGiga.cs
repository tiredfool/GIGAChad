using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance = 2.0f;
    public float height = 1.0f;
    public float damping = 3.0f;
    public float depthOffset = 0f;
    private bool isNegativeDistance = false; // �Ÿ� ����ȭ
    //private Vector3 initialPosition; // �ʱ� ��ġ

    public float shakeAmount = 0.1f; // ��鸲 ����
    public float shakeSpeed = 10.0f; // ��鸲 �ӵ�
    private bool isShaking = false; // ��鸲 ����

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

        void LateUpdate()
    {
        if (!player) return;

        float currentDistance = isNegativeDistance ? -distance : distance; // ���� �Ÿ� ����

        Vector2 desiredPosition = player.position + new Vector3(-currentDistance, height, 0f);
        Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, damping * Time.deltaTime);

        transform.position = new Vector3(smoothedPosition.x + depthOffset, smoothedPosition.y, transform.position.z);
        if (isShaking)
        {
            // ��鸲 ȿ�� ����
            transform.position = transform.position + Random.insideUnitSphere * shakeAmount;
        }
    }

    // �ܺο��� ȣ���Ͽ� �Ÿ��� ������ �����ϴ� �Լ�
    public void SetNegativeDistance(bool value)
    {
        isNegativeDistance = value;

        // SpriteRenderer�� ���� ���� �¿� ����
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = value; 
        }
    }

    public void SetShake(bool value) // ��鸲
    {
        isShaking = value;

      
    }

    public void SetVisible(bool value) // ����ȭ
    {
        gameObject.SetActive(value);
    }
}