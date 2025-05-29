using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 5f;
    private bool hasCollided = false;

    void Start()
    {
        // 중력 적용
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Collider2D 설정 및 Trigger로 변경
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // 2초 뒤 자동 삭제
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCollided && other.CompareTag("Player"))
        {
            hasCollided = true;
            Debug.Log("플레이어가 떨어지는 물체와 충돌!");

            ScoreManager.instance.AddScore(-50);
            Destroy(gameObject);
        }
    }
}
