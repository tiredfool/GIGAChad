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
    public float someHorizontalKnockbackForce = 1f; //���� �˹� ��
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
    private float verticalVelocity; // ���� �ӵ� ���� ���� (���� �ڵ忡�� ������ ����)
    private bool canJump = false; // ���� ���� ��� ����

    public FollowPlayer follower; // ������� �Ⱑ����

    private bool isStackGameMode = false; // ���� ���� ��� ����

    private bool isOnConveyor = false;
    private ConveyorBeltPhysics currentConveyorScript = null;

    private PhysicsMaterial2D defaultMaterial; // �⺻ ����
    public PhysicsMaterial2D wallMaterial; // ���� ������ ����
    private Collider2D playerCollider;

    public GameObject jumpDustEffect; //���� ������
    public Transform dustPoint; //����Ʈ�� ������ ��ġ

    private bool wasGrounded; //���� �����ӿ��� ground ���� ����

    public bool isPlayingFootstepSound = false;
    public float footstepInterval = 0.3f;

    private float horizontalInput; // *** �� ������ ���̽�ƽ/Ű���� �Է¿� ���� ������Ʈ�� ���Դϴ�. ***
    private int currentHorizontalDirection = 0; // -1: ����, 0: ����, 1: ������

    // --- �߰��� ���� ---
    private float fixedYPosition; // Y ���� �� ����� Y ��ġ
    private bool wasPlayerInteractionEnabled = true; // ���� �������� �÷��̾� ��ȣ�ۿ� ����
    // ---
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
        // GameManager �ν��Ͻ��� ���� ��� (�� �ε� ���� ��) ���� ó��
        if (GameManager.instance == null) return;

        // ���� �Է� ó��
        if ((VirtualInputManager.Instance.GetKeyOrButtonDown("Jump") && isGrounded) || (VirtualInputManager.Instance.GetKeyOrButtonDown("Jump") && canJump))
        {
            // GameManager���� �÷��̾� ��ȣ�ۿ��� ��Ȱ��ȭ�Ǿ� ������ ���� �Ұ�
            if (!GameManager.instance.IsPlayerInteractionEnabled()) return;

            jumpRequest = true; // ���� ��û
            canJump = false;
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Jump");
            }
        }

        // �̵� �Է� ���� (FixedUpdate���� ����ϱ� ���� ����)
        // horizontalInput = Input.GetAxisRaw("Horizontal"); // ���� Input.GetAxisRaw �ּ� ó�� �Ǵ� ����

        // VirtualInputManager�� ���� ���� ���� �̵� ������ �����ɴϴ�.
        currentHorizontalDirection = 0;
        if (VirtualInputManager.Instance.GetKeyOrButton("Left"))
        {
            currentHorizontalDirection = -1;
           // Debug.Log("[PlayerController] Left Input Detected!"); // �߰�
        }
        else if (VirtualInputManager.Instance.GetKeyOrButton("Right"))
        {
            currentHorizontalDirection = 1;
          //  //Debug.Log("[PlayerController] Right Input Detected!"); // �߰�
        }

        horizontalInput = currentHorizontalDirection;
        ////Debug.Log($"[PlayerController] currentHorizontalDirection: {currentHorizontalDirection}, horizontalInput: {horizontalInput}"); // �߰�


        // ��������Ʈ ���� ��ȯ
        if (horizontalInput > 0) // currentHorizontalDirection ��� horizontalInput ���
        {
            spriteRenderer.flipX = false;
            follower.SetNegativeDistance(false);
        }
        else if (horizontalInput < 0) // currentHorizontalDirection ��� horizontalInput ���
        {
            spriteRenderer.flipX = true;
            follower.SetNegativeDistance(true);
        }

        // ����� ó�� (�׾��� ��)
        if (died && VirtualInputManager.Instance.GetKeyOrButtonDown("Action") && DialogueManager.instance != null && !DialogueManager.instance.isTyping) // "Action" ���� Ű ���
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

        if (GameManager.instance == null) return;


        bool currentPlayerInteractionEnabled = GameManager.instance.IsPlayerInteractionEnabled();
        bool disableInputByTime = (Time.time < inputDisabledTime);
        bool shouldDisableMovement = disableInputByTime || isStackGameMode || !currentPlayerInteractionEnabled;// ||isTalking;

        if (shouldDisableMovement)
        {

            if (disableInputByTime && isTakingDamage)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else // �� ���� �Է� ��Ȱ��ȭ
            {
                if (wasPlayerInteractionEnabled && !currentPlayerInteractionEnabled)
                {
                    fixedYPosition = transform.position.y;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                    //Debug.Log($"[PlayerController] Y ��ġ ���� ����. ���� Y: {fixedYPosition}");
                }
                transform.position = new Vector3(transform.position.x, fixedYPosition, transform.position.z);
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }
            // �ִϸ��̼� �ӵ��� 0���� �����Ͽ� ���� ���� ����
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false); // ���� �ִϸ��̼ǵ� ����
            animator.SetBool("IsGround", true); // �� �ִϸ��̼� ����

            // TakeDamage �ִϸ��̼� Ʈ���� �ʱ�ȭ (�Է� �Ұ� ��)
            animator.ResetTrigger("TakeDamage");

            wasPlayerInteractionEnabled = currentPlayerInteractionEnabled; // ���� ������Ʈ
            return;
        }
        else
        {
            // �������� �ٽ� Ȱ��ȭ�Ǵ� ���� Y ��ġ ���� ����
            if (!wasPlayerInteractionEnabled && currentPlayerInteractionEnabled) // ��� Ȱ��ȭ ���°� �� ���
            {
                rb.gravityScale = 1f; // �߷� �ٽ� ����
                //Debug.Log("[PlayerController] Y ��ġ ���� ���� �� �߷� ����.");
            }

            wasPlayerInteractionEnabled = currentPlayerInteractionEnabled; // ���� ������Ʈ
            wasGrounded = isGrounded; // ���� Grounded ���� ����
            isGrounded = IsGrounded();

            if (!wasGrounded && isGrounded)
            {
                if (jumpDustEffect != null && dustPoint != null)
                {
                    GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                    Destroy(dust, 0.2f);
                }
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.PlaySFX("Lend");
                }
            }

            // �ִϸ��̼� ���� (����/��)
            animator.SetBool("IsJumping", !isGrounded);
            animator.SetBool("IsGround", isGrounded);

            // ���� ó��
            if (jumpRequest)
            {
                jumpRequest = false; // ���� ��û �ʱ�ȭ
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);

                // ���� �� ����Ʈ ����
                if (jumpDustEffect != null && dustPoint != null)
                {
                    GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                    Destroy(dust, 0.2f);
                }

                // ���� �� �߼Ҹ� ���� (�̹� �� ó���Ǿ� ������, ��Ȯ��)
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

            // ���߿��� �ӵ� ���̱� (���� �ڵ� ����)
            if (!isGrounded && !isTalking && !isStackGameMode) // isTalking�� isStackGameMode�� shouldDisableMovement���� �̹� ó����
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            }

            // �÷��̾� �̵� (���ӵ� ���)
            if (horizontalInput != 0) // �Է��� ���� �� (���� horizontalInput�� -1, 0, 1 ���� ����)
            {
                // �÷��̾��� ���� ���� �ӵ��� �ִ� �ӵ� ���� ���� �ִ��� Ȯ��
                // �׸��� ��ǥ ���������� ������ �������� Ȯ��
                if (Mathf.Abs(rb.velocity.x) < maxMoveSpeed || Mathf.Sign(rb.velocity.x) != Mathf.Sign(horizontalInput))
                {
                    rb.AddForce(Vector2.right * horizontalInput * accelerationForce, ForceMode2D.Force);
                }
            }
            else if (isGrounded && !isOnConveyor) // �Է��� ����, ���� �ְ�, �����̾� ���� �ƴ� ��
            {
                // ���� ���� ���� ���� �ӵ��� ������ 0���� (����)
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.fixedDeltaTime * 10f), rb.velocity.y);
            }

            // �����̾� ��Ʈ �� ����
            if (isOnConveyor && currentConveyorScript != null)
            {
                Vector2 conveyorPushDirection = currentConveyorScript.transform.right * currentConveyorScript.directionMultiplier;
                rb.AddForce(conveyorPushDirection * currentConveyorScript.pushForce, ForceMode2D.Force);
            }

            // �ִϸ��̼� ���� (�̵� �ӵ�)
            // animator.SetFloat("Speed", Mathf.Abs(horizontalInput)); // �Է� �� ��� (���� �ڵ� ����)
            // �ִϸ��̼� Speed�� ���� �̵� �ӵ��� �����ִ� ���� �� �ڿ��������ϴ�.
            // ���̽�ƽ/Ű���� �Է��� ���� �� animator.SetFloat("Speed", 1f) (������); ���� �� 0f (����)
            // �Ǵ� Mathf.Abs(rb.velocity.x)�� ���
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x) > 0.1f ? 1f : 0f); // ���� ����

            // �߼Ҹ� ��� ���� (����: ���� �ӵ� ��� �Է� ���, �̹� ���� ����)
            bool shouldPlayFootsteps = isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f; // ���⼭ rb.velocity.x�� ����ϴ� ���� �� ��Ȯ�� �� �ֽ��ϴ�.

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
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.8f)
                {
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
            TakeDamage(collision.transform.position);
        }
        isGrounded = IsGrounded();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = IsGrounded();
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("�÷��̾ �¾ҽ��ϴ�!");

            if (rb.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                //Debug.Log("���");
                Enemy_Move enemy = collision.gameObject.GetComponent<Enemy_Move>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);
                    rb.velocity = new Vector2(rb.velocity.x, 3f);
                    isGrounded = IsGrounded();
                    canJump = true;
                    verticalVelocity = 0;
                    //Debug.Log("�� ���� �� isGrounded: " + isGrounded);
                }
            }
            else
            {
                TakeDamage(collision.transform.position);
            }
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            TakeDamage(collision.transform.position);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick")
           || collision.gameObject.CompareTag("Slow") || collision.gameObject.CompareTag("Move"))
        {
            isGrounded = IsGrounded();
        }
    }

    public void TakeDamage(Vector2 enemyPosition)
    {
        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage && !isStackGameMode && !died)
        {
            if (GameManager.instance != null)
            {
                if (GameManager.instance.getIndex() == 0)
                {
                    return;
                }
            }
            health -= 10;
            //Debug.Log("Player Health: " + health);
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.SetHealth(health);
            }

            // ���� �˹� ���� ��� (�����Լ� �־����� ����)
            float knockbackDirection = (transform.position.x > enemyPosition.x) ? 1 : -1;
            Vector2 knockbackForce = new Vector2(knockbackDirection * someHorizontalKnockbackForce, knockbackVerticalSpeed);
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);

            inputDisabledTime = Time.time + inputDisableDuration;

            animator.SetTrigger("TakeDamage");

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

    public void TakeDamage()
    {

        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage && !isStackGameMode && !died)
        {
            health -= 10;
            //Debug.Log("Player Health: " + health);
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.SetHealth(health);
            }

            rb.AddForce(Vector2.up * knockbackVerticalSpeed, ForceMode2D.Impulse); // ���� �˹鸸 ����

            inputDisabledTime = Time.time + inputDisableDuration;

            animator.SetTrigger("TakeDamage");

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


    IEnumerator BlinkEffect()
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
        if (MainSoundManager.instance != null) MainSoundManager.instance.StopAllSFX();
        //Debug.Log("Player Died!");
        GameManager.instance.totalLives--;
        PlayerPrefs.SetInt("TotalLives", GameManager.instance.totalLives);
        GameManager.instance.UpdateLifeUI();
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.StopBGM();
            MainSoundManager.instance.PlayBGM("gameOver");
        }
        Time.timeScale = 0;

        if (GameManager.instance.totalLives <= 0)
        {
            //Debug.Log("Game Over");
            game_over = true;
            if (DialogueManager.instance != null) // DialogueManager �ν��Ͻ� Ȯ��
            {
                DialogueManager.instance.SetDiedMessage("Game Over");
            }
            Destroy(gameObject);
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            if (DialogueManager.instance != null) // DialogueManager �ν��Ͻ� Ȯ��
            {
                DialogueManager.instance.SetDiedMessage("oh..����� �� �� �� �־� \n\n\n\n\n\n\n\n\n 'R'�� ���� �ٽ� Stand Up �ϴ°ž�!");
            }
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
        //Debug.Log("���𰡿� �浹 ������: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Coin"))
        {
            //Debug.Log("���� Ʈ���� �浹 ������!");

            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(1);
            }

            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Coin");
                //Debug.Log("���� ���� ���");
            }

            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Finish")
        {
            if (GameManager.instance != null) // GameManager �ν��Ͻ� Ȯ��
            {
                GameManager.instance.NextStage();
            }
        }

        if (collision.gameObject.CompareTag("Slow"))
        {
            ApplySlow(0.5f);
        }

        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            isOnConveyor = true;
            currentConveyorScript = collision.GetComponent<ConveyorBeltPhysics>();
            if (currentConveyorScript == null)
            {
                //Debug.LogError("ConveyorBeltPhysics ��ũ��Ʈ�� ã�� �� �����ϴ�!", collision.gameObject);
            }
            //Debug.Log("Entered Conveyor");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Slow"))
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
                //Debug.Log("Exited Conveyor");
            }
        }
    }

    public void ApplySlow(float multiplier)
    {
        maxMoveSpeed = originalMoveSpeed * multiplier;
        //Debug.Log("�̵� �ӵ� ����: " + maxMoveSpeed);
    }

    public void RemoveSlow()
    {
        maxMoveSpeed = originalMoveSpeed;
        //Debug.Log("�̵� �ӵ� ����: " + maxMoveSpeed);
    }

    void RestartGame()
    {
        //Debug.Log("���� ����!");


        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.setBlack();
            //Debug.Log("ȭ���� �˰� �����߽��ϴ�.");
        }
        else
        {
            //Debug.LogError("DialogueManager �ν��Ͻ��� ã�� �� �����ϴ�! ȭ���� �˰� ���� �� �����ϴ�.");

        }


        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateLifeUI();
        }


        SceneManager.LoadSceneAsync("SampleScene").completed += (AsyncOperation operation) =>
        {
            //Debug.Log("�� �ε� �Ϸ�: " + operation.isDone);


            if (GameManager.instance != null)
            {
                if (GameManager.instance.getIndex() < GameManager.instance.startPositions.Length)
                {

                    health = 100f;
                    isTakingDamage = false;

                    if (DialogueManager.instance != null)
                    {
                        DialogueManager.instance.diedText.text = "";
                    }

                    GameManager.instance.FindPlayer();
                    GameManager.instance.PlayerReposition();
                    GameManager.instance.FindAndSetStagesByParent();
                    GameManager.instance.ResetStageActivation();

                    if (DialogueManager.instance != null)
                    {
                        DialogueManager.instance.SetHealth(health); // DialogueManager�� ü�� UI ������Ʈ
                    }
                }
                GameManager.instance.UpdateLifeUI();
            }

            Time.timeScale = 1;
            MainSoundManager.instance.StopBGM();
            if (GameManager.index <= 7) MainSoundManager.instance.PlayBGM("Basic");
            else if (GameManager.index <= 14) MainSoundManager.instance.PlayBGM("2stage");

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.FadeFromBlack();
                //Debug.Log("ȭ���� ��� ��ȯ �����߽��ϴ�.");
            }
            //if(SwitchZone.Instance != null)
            //{
            //    if (!SwitchZone.Instance.gameObject.activeSelf)
            //    {
            //        SwitchZone.Instance.gameObject.SetActive(true);
            //    }
            //}
        };
    }

    public void SetStackGameMode(bool isActive)
    {
        isStackGameMode = isActive;

        if (isActive)
        {
            inputDisabledTime = float.MaxValue;
            isTakingDamage = true;
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGround", true);
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
            inputDisabledTime = Time.time;
            isTakingDamage = false;
        }
    }

    IEnumerator PlayFootstepSound()
    {
        // �ȴ� �Ҹ��� �÷��̾ ������ ������ ���� ��� (rb.velocity.x ���)
        while (isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f && !isTalking && !isStackGameMode && GameManager.instance != null && GameManager.instance.IsPlayerInteractionEnabled())
        {
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Footstep");
            }
            yield return new WaitForSeconds(footstepInterval);
        }
        isPlayingFootstepSound = false;
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.StopSFX("Footstep");
        }
    }


    public void SetCanMove(bool state)
    {

        if (state)
        {
            // ������ Ȱ��ȭ �� �ִϸ����� �ʱ�ȭ
            animator.ResetTrigger("TakeDamage");
        }
    }
}