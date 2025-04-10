using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10f;
    private Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) // 🛑 Rigidbody2D가 없을 경우 경고 메시지 출력
        {
            Debug.LogError("Bullet 스크립트에 Rigidbody2D가 없습니다! Rigidbody2D를 추가하세요.", gameObject);
        }
    }
    void Update()
    {
    
    }

    public void SetDirection(Vector3 dir)
    {
        dir.z = 0;
        rb.velocity = dir.normalized*speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 장애물 또는 적에 맞으면 총알 삭제
        if (other.CompareTag("Food") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
