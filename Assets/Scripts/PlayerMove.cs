using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float health = 100f;
    public float damageCooldown = 0.5f; // 피격 무적 시간
    public float knockbackVerticalSpeed = 5f; // 수직 넉백 속도
    public float blinkDuration = 0.1f; // 깜빡이는 간격
    public int blinkCount = 5; // 깜빡이는 횟수
    public float inputDisableDuration = 0.3f; // 입력 막는 시간
    private float lastDamageTime;
    private float inputDisabledTime;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isTakingDamage = false;
    private bool isTalking = false;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastDamageTime = -damageCooldown; // 시작 시 무적 상태로 설정
    }

    void Update()
    {
        // 입력이 막혀있는지 확인
        if (Time.time < inputDisabledTime || isTalking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // 움직임 멈춤
            return; // 입력 처리 X
        }
        else
        {
            animator.ResetTrigger("TakeDamage");
        }

        // 좌우 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        Vector2 moveVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;

        // 스프라이트 방향 전환
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // 점프
        if (Input.GetButton("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsGround", false);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Spike") || collision.gameObject.CompareTag("Enemy")) && (Time.time - lastDamageTime > damageCooldown) && !isTakingDamage)
        {
            TakeDamage();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick"))
        {
            isGrounded = true;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGround", true);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("플레이어가 맞았습니다!");

            // Attack
            if (rb.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                Debug.Log("밟기");
                // 적 밟기 로직 (여기에 적 데미지 처리)
                Enemy_Move enemy = collision.gameObject.GetComponent<Enemy_Move>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);
                    rb.velocity = new Vector2(rb.velocity.x, 3f);
                    isGrounded = true;
                }
            }
            // Damaged
            else
            {
                TakeDamage(); // 몬스터에게 맞으면 데미지
            }
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            TakeDamage();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick"))
        {
            isGrounded = false;
            animator.SetBool("IsGround", false);
        }
    }

    public void TakeDamage()
    {
        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage)
        {
            // 피격 쿨다운 적용
            health -= 1;
            Debug.Log("Player Health: " + health);

            rb.velocity = new Vector2(rb.velocity.x, knockbackVerticalSpeed);//넉백
            inputDisabledTime = Time.time + inputDisableDuration;//입력불가 시간

            animator.SetTrigger("TakeDamage");//피격 애니

            lastDamageTime = Time.time;
            if (health <= 0)
            {
                Die();
            }

            StartCoroutine(BlinkEffect());
        }
    }

    IEnumerator BlinkEffect() // 데미지시 깜빡이
    {
        isTakingDamage = true;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkDuration);
        }

        isTakingDamage = false;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        Destroy(gameObject);
    }

    public bool IsFalling()
    {
        return rb.velocity.y < 0;
    }

    public void SetTalking(bool talking)
    {
        isTalking = talking;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsGround", true);
    }
}
