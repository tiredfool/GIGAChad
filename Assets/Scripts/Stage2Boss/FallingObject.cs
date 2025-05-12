using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 5f;
    private bool hasCollided = false;

    void Start()
    {
        // �߷� ����
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Collider2D ���� �� Trigger�� ����
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // 2�� �� �ڵ� ����
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
            Debug.Log("�÷��̾ �������� ��ü�� �浹!");

            ScoreManager.instance.AddScore(-50);
            Destroy(gameObject);
        }
    }
}
