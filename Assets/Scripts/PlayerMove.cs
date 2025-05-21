using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : MonoBehaviour
{
    //public float moveSpeed = 5f;

    public float accelerationForce = 50f; // �÷��̾� �̵� ���ӵ� (���� �߰�)
    public float maxMoveSpeed = 5f; // �÷��̾� �ִ� �̵� �ӵ� (moveSpeed ��� ���)

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

    private bool died = false;
    private bool game_over = false;


    public float groundCheckDistance = 0.2f; // �� üũ Raycast �Ÿ� (�� ����)
    public LayerMask groundLayer; // �� ���̾�
    private bool jumpRequest; // ���� ��û ����
    private float verticalVelocity; // ���� �ӵ� ���� ����
    private bool canJump = false;

    public FollowPlayer follower; // ������� �Ⱑ����

    private bool isStackGameMode = false; // ���� ���� ��� ����

    private bool isOnConveyor = false;
    private ConveyorBeltPhysics currentConveyorScript = null;

    private PhysicsMaterial2D defaultMaterial; // �⺻ ����
    public PhysicsMaterial2D wallMaterial; // ���� ������ ����
    private Collider2D playerCollider;

    public GameObject jumpDustEffect;//���� ������
    public Transform dustPoint;//����Ʈ�� ������ ��ġ

    private bool wasGrounded;//���� �����ӿ��� ground ���� ����

    public bool isPlayingFootstepSound = false;
    public float footstepInterval = 0.3f;

    private float horizontalInput; // ���׸ӽſ�
    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        defaultMaterial = playerCollider.sharedMaterial;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastDamageTime = -damageCooldown; // ���� �� ���� ���·� ����
        originalMoveSpeed = maxMoveSpeed; // ���� �ӵ� ����
    }

    void Update()
    {
        // ���� �Է� ó��
        if ((Input.GetButtonDown("Jump") && isGrounded) || (Input.GetButtonDown("Jump") && canJump))
        {
            jumpRequest = true; // ���� ��û
            canJump = false;
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Jump");
            }
            // animator.SetBool("IsJumping", true);
            // animator.SetBool("IsGround", false);
        }
        float moveInput = Input.GetAxisRaw("Horizontal");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        // ��������Ʈ ���� ��ȯ
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
            follower.SetNegativeDistance(false);
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
            follower.SetNegativeDistance(true);
        }

        if (died && Input.GetKeyDown(KeyCode.R) && !DialogueManager.instance.isTyping)
        {

            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }

            died = false;
            RestartGame();
        }

    }

    void FixedUpdate()
    {
        // �Է��� �����ִ��� Ȯ��
        if (Time.time < inputDisabledTime || isTalking || isStackGameMode)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // ������ ����

            // �� �κ��� FixedUpdate���� �������� ������ �� �߼Ҹ��� ���ߵ��� �̹� �� ó���Ǿ� �ֽ��ϴ�.
            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }

            return; // �Է� ó�� X
        }
        else
        {
            animator.ResetTrigger("TakeDamage");
        }
        wasGrounded = isGrounded;//���� Grounded ���� ����
        // �� üũ (FixedUpdate���� Raycast ���)

        isGrounded = IsGrounded();

        //������ ����Ʈ �߻�
        if (!wasGrounded && isGrounded)
        {
            if (jumpRequest != null && dustPoint != null) // jumpRequest�� �׻� true/false �οﰪ�̹Ƿ� null �񱳴� �ǹ� �����ϴ�.
                                                          // ���� �����ӿ� ���� ��û�� �־������� Ȯ���ϴ� ���� �� �����մϴ�.
                                                          // ���⼭�� `wasGrounded`�� false�̰� `isGrounded`�� true�� ��쿡�� ����Ʈ�� �����ϴ� ������ ����մϴ�.
            {
                GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                Destroy(dust, 0.2f);
            }
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Lend");
            }
        }

        //  if(isGrounded) Debug.Log("��");
        // �ִϸ��̼� ����
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsGround", isGrounded);

        // ���� ó��
        if (jumpRequest)
        {
            jumpRequest = false; // ���� ��û �ʱ�ȭ
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            //���� �� ����Ʈ ����
            if (jumpDustEffect != null && dustPoint != null)
            {
                GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                Destroy(dust, 0.2f);
            }

            // ���� �� �߼Ҹ� ����
            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }
        }
        if (!isGrounded && !isTalking && !isStackGameMode)
        {
            //���߿��� �ӵ� ���̱�
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y); //
        }

        // �¿� �̵�
        //float moveInput = Input.GetAxisRaw("Horizontal");
        //bool allowPlayerVelocityOverride = true; // �÷��̾� �Է����� �ӵ��� ����� ����


        //if (isOnConveyor && currentConveyorScript != null)
        //{
        //    // �����̾�� ���� �������� �̵� ������ Ȯ��
        //    if (moveInput != 0 && Mathf.Sign(moveInput) == Mathf.Sign(currentConveyorScript.directionMultiplier))
        //    {
        //        allowPlayerVelocityOverride = false;
        //        float extraPushFactor = 4f; // moveSpeed�� �ٸ� �� ���, Ʃ�� �ʿ�
        //        rb.AddForce(Vector2.right * moveInput * extraPushFactor, ForceMode2D.Force);

        //    }
        //}

        //if ((allowPlayerVelocityOverride && Mathf.Abs(moveInput) > 0.01f)) // �Է��� ���� �� (0�� �ƴ� ��)
        //{
        //    rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        //}
        //else if (allowPlayerVelocityOverride && Mathf.Abs(moveInput) <= 0.01f) // �Է��� ���� �� (�߰�: ���� �� �߼Ҹ� ��)
        //{
        //    // �Է��� ������ ĳ���Ͱ� ���߰� �߼Ҹ��� ����� �մϴ�.
        //    // ��, �����̾� ��Ʈ ���� �ְų� �ٸ� �ܺ� ���� ���� �����̴� ���� ���ܷ� �־� �� ���� �ֽ��ϴ�.
        //    // ���⼭�� �ܼ��� �Է��� ������ ���� �ӵ��� 0���� ����ϴ�.
        //    rb.velocity = new Vector2(0, rb.velocity.y);
        //}


        //animator.SetFloat("Speed", Mathf.Abs(moveInput));

        //// �߼Ҹ� ��� ���� ��ȭ: ������ �Է��� 0.1���� Ŀ�� �ϰ�, ���� ��� �־�� �ϸ�, ��ȭ ���� �ƴϰ�, ���� ���� ��尡 �ƴ� ��
        //bool shouldPlayFootsteps = isGrounded && Mathf.Abs(moveInput) > 0.1f && !isTalking && !isStackGameMode;

        //if (shouldPlayFootsteps && !isPlayingFootstepSound)
        //{
        //    isPlayingFootstepSound = true;
        //    StartCoroutine("PlayFootstepSound");
        //}
        //else if (!shouldPlayFootsteps && isPlayingFootstepSound)
        //{
        //    isPlayingFootstepSound = false;
        //    StopCoroutine("PlayFootstepSound");
        //    if (MainSoundManager.instance != null)
        //    {
        //        MainSoundManager.instance.StopSFX("Footstep");
        //    }
        //}
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (Time.time >= inputDisabledTime && !isTalking && !isStackGameMode)
        {
            // �÷��̾��� ���� ���� �ӵ��� �ִ� �ӵ� ���� ���� �ִ��� Ȯ��
            // �׸��� ��ǥ ���������� ������ �������� Ȯ��
            if (Mathf.Abs(rb.velocity.x) < maxMoveSpeed || Mathf.Sign(rb.velocity.x) != Mathf.Sign(horizontalInput))
            {
                rb.AddForce(Vector2.right * horizontalInput * accelerationForce, ForceMode2D.Force);
            }
            // �Է��� ������ ���� (���� ����, �ʿ信 ���� �ּ� ó��)
            else if (horizontalInput == 0 && isGrounded && !isOnConveyor)
            {
                // ���� ���� ���� ���� �ӵ��� ������ 0����
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.fixedDeltaTime * 10f), rb.velocity.y);
            }
        }
        //else
        //{
        //    // �Է��� ��Ȱ��ȭ�� ���, �÷��̾��� ���� �ӵ��� 0����
        //    // (�����̾� ��Ʈ�� ������ ���� �ʵ���)
        //    rb.velocity = new Vector2(0, rb.velocity.y);
        //}

        // �����̾� ��Ʈ �� ����
        if (isOnConveyor && currentConveyorScript != null)
        {
            Vector2 conveyorPushDirection = currentConveyorScript.transform.right * currentConveyorScript.directionMultiplier;
            rb.AddForce(conveyorPushDirection * currentConveyorScript.pushForce, ForceMode2D.Force);
        }

      
        //// ���� ���߿��� ������ ���ߴ� ������ ���ϸ� ����
        // if (!isGrounded && !isTalking && !isStackGameMode)
        // {
        //     rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
        // }

        // �ִϸ��̼� ����
        //animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x)); // ���� �ӵ� ���
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        // �߼Ҹ� ��� ���� (actual velocity ���)
        bool shouldPlayFootsteps = isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f && !isTalking && !isStackGameMode;

        if (shouldPlayFootsteps && !isPlayingFootstepSound)
        {
            isPlayingFootstepSound = true;
            StartCoroutine("PlayFootstepSound");
        }
        else if (!shouldPlayFootsteps && isPlayingFootstepSound)
        {
            isPlayingFootstepSound = false;
            StopCoroutine("PlayFootstepSound");
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.StopSFX("Footstep");
            }
        }
    

}

    bool IsGrounded()
    {
        // ĳ���� �� ������ Raycast�� ��Ƽ� ���� ��Ҵ��� Ȯ��
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }


    void OnCollisionStay2D(Collision2D collision)
    {
        //�� �ٱ� ����
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {

                if (Mathf.Abs(contact.normal.x) > 0.8f)
                {
                    // float moveInput = Input.GetAxisRaw("Horizontal"); // ���⼭ moveInput�� ������� �����Ƿ� �����ص� �����մϴ�.
                    // Debug.Log("wall");
                    playerCollider.sharedMaterial = wallMaterial;
                }
                else
                {
                    playerCollider.sharedMaterial = defaultMaterial;
                }
            }
        }
        if ((collision.gameObject.CompareTag("Spike") || collision.gameObject.CompareTag("Enemy")) && (Time.time - lastDamageTime > damageCooldown) && !isTakingDamage && !died)
        {
            TakeDamage();
        }
        isGrounded = IsGrounded();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = IsGrounded();
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
                    isGrounded = IsGrounded(); // ���� �� �ٽ� �� üũ
                    canJump = true;
                    verticalVelocity = 0; // verticalVelocity�� ������ �ʴ� ���� �����ϴ�.
                    Debug.Log("�� ���� �� isGrounded: " + isGrounded);
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
            isGrounded = IsGrounded();
            Debug.Log("OnCollisionExit2D ����");
        }
    }

    public void TakeDamage()
    {
        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage && !isStackGameMode && !died)
        {
            // �ǰ� ��ٿ� ����
            health -= 10;
            Debug.Log("Player Health: " + health);
            DialogueManager.instance.SetHealth(health);
            rb.velocity = new Vector2(rb.velocity.x, knockbackVerticalSpeed);//�˹�
            inputDisabledTime = Time.time + inputDisableDuration;//�ԷºҰ� �ð�

            animator.SetTrigger("TakeDamage");//�ǰ� �ִ�

            lastDamageTime = Time.time;
            if (health <= 0)
            {
                if (isPlayingFootstepSound)
                {
                    isPlayingFootstepSound = false;
                    StopCoroutine("PlayFootstepSound");
                    if (MainSoundManager.instance != null)
                    {
                        MainSoundManager.instance.StopSFX("Footstep");
                    }
                }
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

    public void Die()
    {
        died = true;
        MainSoundManager.instance.StopAllSFX();
        Debug.Log("Player Died!");
        GameManager.instance.totalLives--;  // ��� �ϳ� ����
        PlayerPrefs.SetInt("TotalLives", GameManager.instance.totalLives);  // PlayerPrefs�� ����
        GameManager.instance.UpdateLifeUI();  // UI ������Ʈ
        Time.timeScale = 0;
       
        if (GameManager.instance.totalLives <= 0)
        {
            // ����� 0�� ��� ���� ���� ó��
            Debug.Log("Game Over");
            game_over = true;
            DialogueManager.instance.SetDiedMessage("Game Over");
            Destroy(gameObject);  // �÷��̾� ������Ʈ ����
        }
        else
        {

            GetComponent<Renderer>().enabled = false;
            DialogueManager.instance.SetDiedMessage("oh..����� �� �� �� �־� \n\n\n\n\n\n\n\n\n 'R'�� ���� �ٽ� Stand Up �ϴ°ž�!");
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

        // ��ȭ ����/���� �� �߼Ҹ� �ڷ�ƾ ����
        if (isPlayingFootstepSound)
        {
            isPlayingFootstepSound = false;
            StopCoroutine("PlayFootstepSound");
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.StopSFX("Footstep");
            }
        }
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

        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            isOnConveyor = true;
            currentConveyorScript = collision.GetComponent<ConveyorBeltPhysics>(); // ��ũ��Ʈ �̸� Ȯ��!
            if (currentConveyorScript == null)
            {
                Debug.LogError("ConveyorBeltPhysics2D ��ũ��Ʈ�� ã�� �� �����ϴ�!", collision.gameObject);
            }
            Debug.Log("Entered Conveyor");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Slow")) // Slow�� ����� �ӵ� ����
        {
            RemoveSlow();
        }


        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            ConveyorBeltPhysics exitedConveyor = collision.GetComponent<ConveyorBeltPhysics>();
            if (currentConveyorScript == exitedConveyor)
            {
                isOnConveyor = false;
                currentConveyorScript = null;
                Debug.Log("Exited Conveyor");
            }
        }
    }

    public void ApplySlow(float multiplier)
    {
        maxMoveSpeed = originalMoveSpeed * multiplier;
        Debug.Log("�̵� �ӵ� ����: " + maxMoveSpeed);
    }

    public void RemoveSlow()
    {
        maxMoveSpeed = originalMoveSpeed;
        Debug.Log("�̵� �ӵ� ����: " + maxMoveSpeed);
    }

    void RestartGame()
    {
        Debug.Log("���½���!");

        DialogueManager.instance.SetDiedMessage("");
        GameManager.instance.UpdateLifeUI();
        SceneManager.LoadSceneAsync(0).completed += (AsyncOperation operation) =>
        {
            // �� �ε尡 �Ϸ�� �� ����Ǵ� �ڵ�
            if (GameManager.instance != null && GameManager.instance.stageIndex < GameManager.instance.startPositions.Length)
            {
                health = 100f;
                isTakingDamage = false;
                GameManager.instance.FindPlayer();
                GameManager.instance.PlayerReposition();
                GameManager.instance.FindAndSetStagesByParent();
                GameManager.instance.ResetStageActivation();
                DialogueManager.instance.ReloadBlack();
                DialogueManager.instance.SetHealth(health);
                DialogueManager.instance.diedText.text = "";
                // �� �ε� �Ϸ� �� ī�޶� Confiner �缳��
                //GameManager.instance.GetComponent<GameManager>().Invoke("SetCameraConfinerForCurrentStage", 0.15f);
            }
            GameManager.instance.UpdateLifeUI();
            Time.timeScale = 1;
        };
    }
    // ���� ���� ��ȯ �� �۵�
    public void SetStackGameMode(bool isActive)
    {
        isStackGameMode = isActive;

        if (isActive)
        {
            inputDisabledTime = float.MaxValue; // �Է� ����
            isTakingDamage = true;              // ���� ó��
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGround", true);
            // ���� ���� ��� ���� �� �߼Ҹ� ����
            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }
        }
        else
        {
            inputDisabledTime = Time.time;      // �Է� ��Ȱ��ȭ
            isTakingDamage = false;
        }
    }

    IEnumerator PlayFootstepSound() // �����Ҹ� 
    {
        // �ڷ�ƾ ���ο��� ���������� ���� �Է� ���¸� Ȯ���մϴ�.
        while (isGrounded && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f && !isTalking && !isStackGameMode)
        {
            // SoundManager�� �����ϰ� Footstep ���� ��� �Լ��� ���� ��� ȣ��
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Footstep"); // "Footstep"�� ���� Ŭ���� �̸� �Ǵ� �ĺ����Դϴ�.
            }
            yield return new WaitForSeconds(footstepInterval);
        }
        // while ���� ������ false�� �Ǿ� �ڷ�ƾ�� ����� ��,
        // isPlayingFootstepSound ���¸� false�� ������Ʈ�ϰ� ���带 �����մϴ�.
        isPlayingFootstepSound = false;
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.StopSFX("Footstep");
        }
    }
}