using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Move : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator ani;
    SpriteRenderer sprite;
    public float health = 1f;
    public float fadeOutDuration = 0.5f;
    public string playerTag = "Player";
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    public int nextMove;
    private bool isDead = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
         if (spriteRenderer == null)
        {
             spriteRenderer = GetComponent<SpriteRenderer>();

            // spriteRenderer 초기화 확인
        if (spriteRenderer == null)
        {
             Debug.LogError("SpriteRenderer is null!");
             Destroy(this);
        }
        }

        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        //Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Ground"));
        if (rayHit.collider == null)
        {
            //Debug.Log("경고! 이 앞은 낭떨어지.");
            nextMove *= -1;
            CancelInvoke(); 
            Invoke("Think", 5);
        }
    }

    //재귀 함수
    void Think()
    {
        //Set Next Active
        nextMove = Random.Range(-1, 2);

        //Sprite Animation
        ani.SetInteger("WalkSpeed", nextMove);
        
        //Flip Sprite
        if(nextMove != 0)
            sprite.flipX = nextMove == 1;

        //Recursive
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        sprite.flipX = nextMove == 1;

        CancelInvoke();
        Invoke("Think", 2);
    }

    public void OnDamaged()
    {
        
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.IsFalling())
            {
                StartCoroutine(FadeOut());
            }
            else
            {
               // player.TakeDamage(1); //플레이어 스크립트에서 처리중
            }
        }
    }

    IEnumerator FadeOut()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is null!");
            yield break; // 코루틴 종료
        }
        GetComponent<Collider2D>().enabled = false;
        isDead = true;

        float startAlpha = spriteRenderer.color.a;
        float time = 0;

        while (time < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }
        Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if(!isDead) StartCoroutine(FadeOut());
        }
    }
}