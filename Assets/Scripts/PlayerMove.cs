using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float originalMoveSpeed; // ���� �ӵ��� ������ ����
    public float jumpForce = 10f;
    public float health = 100f;
    public float damageCooldown = 0.5f; // �ǰ� ���� �ð�
    public float knockbackVerticalSpeed = 5f; // ���� �˹� �ӵ�
    public float blinkDuration = 0.1f; // �����̴� ����
    public int blinkCount = 5; // �����̴� Ƚ��
    public float inputDisableDuration = 0.3f; // �Է� ���� �ð�
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
        lastDamageTime = -damageCooldown; // ���� �� ���� ���·� ����
        originalMoveSpeed = moveSpeed; // ���� �� ���� �ӵ� ����
    }

    void Update()
    {
        // �Է��� �����ִ��� Ȯ��
        if (Time.time < inputDisabledTime || isTalking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // ������ ����
            return; // �Է� ó�� X
        }
        else
        {
            animator.ResetTrigger("TakeDamage");
        }

        // �¿� �̵�
        float moveInput = Input.GetAxisRaw("Horizontal");
        Vector2 moveVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;

        // ��������Ʈ ���� ��ȯ
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // ����
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
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick") 
            || collision.gameObject.CompareTag("Slow") || collision.gameObject.CompareTag("Move"))
        {
            isGrounded = true;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGround", true);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("�÷��̾ �¾ҽ��ϴ�!");

            // Attack
            if (rb.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                Debug.Log("���");
                // �� ��� ���� (���⿡ �� ������ ó��)
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
                TakeDamage(); // ���Ϳ��� ������ ������
            }
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            TakeDamage();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick") 
            || collision.gameObject.CompareTag("Slow") || collision.gameObject.CompareTag("Move"))
        {
            isGrounded = false;
            animator.SetBool("IsGround", false);
        }
    }

    public void TakeDamage()
    {
        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage)
        {
            // �ǰ� ��ٿ� ����
            health -= 50;
            Debug.Log("Player Health: " + health);

            rb.velocity = new Vector2(rb.velocity.x, knockbackVerticalSpeed);//�˹�
            inputDisabledTime = Time.time + inputDisableDuration;//�ԷºҰ� �ð�

            animator.SetTrigger("TakeDamage");//�ǰ� �ִ�

            lastDamageTime = Time.time;
            if (health <= 0)
            {
                Die();
            }

            StartCoroutine(BlinkEffect());
        }
    }

    IEnumerator BlinkEffect() // �������� ������
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
        GameManager.instance.totalLives--;  // ��� �ϳ� ����
        PlayerPrefs.SetInt("TotalLives", GameManager.instance.totalLives);  // PlayerPrefs�� ����
        GameManager.instance.UpdateLifeUI();  // UI ������Ʈ

        if (GameManager.instance.totalLives <= 0)
        {
            // ����� 0�� ��� ���� ���� ó��
            Debug.Log("Game Over");
            Destroy(gameObject);  // �÷��̾� ������Ʈ ����
        }
        else
        {
            // ����� ���������� �÷��̾� ��ġ �ʱ�ȭ
            RestartGame();
        }
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("���𰡿� �浹 ������: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log("���� Ʈ���� �浹 ������!");

            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(1);  // GameManager���� ���� ���� ���� 1�� ����
            }

            Destroy(collision.gameObject);  // ���� ����
        }

        if (collision.gameObject.tag == "Finish")
        { 
            GameManager.instance.NextStage();
        }

        if (collision.gameObject.CompareTag("Slow")) // 'Slow' �±׸� ���� ������Ʈ�� ������ ����
        {
            ApplySlow(0.5f); // 50% �ӵ� ����
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Slow")) // Slow�� ����� �ӵ� ����
        {
            RemoveSlow();
        }
    }

    public void ApplySlow(float multiplier)
    {
        moveSpeed = originalMoveSpeed * multiplier;
        Debug.Log("�̵� �ӵ� ����: " + moveSpeed);
    }

    public void RemoveSlow()
    {
        moveSpeed = originalMoveSpeed;
        Debug.Log("�̵� �ӵ� ����: " + moveSpeed);
    }

    void RestartGame()
    {
        // ���� ó�� ���·� ����
        SceneManager.LoadScene(0);  // �� 0��(ù ��° ��)���� �ε�
        Time.timeScale = 1;  // �ð��� ���� ���¶�� �ٽ� ���� �帧���� ����
    }
}
