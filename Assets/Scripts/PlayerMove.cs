using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float originalMoveSpeed; // 원래 속도를 저장할 변수
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

    private bool died = false;
    private bool game_over = false;


    public float groundCheckDistance = 0.2f; // 땅 체크 Raycast 거리 (값 증가)
    public LayerMask groundLayer; // 땅 레이어
    private bool jumpRequest; // 점프 요청 변수
    private float verticalVelocity; // 수직 속도 저장 변수
    private bool canJump = false;

    public FollowPlayer follower; // 따라오는 기가차드

    private bool isStackGameMode = false; // 스택 게임 모드 여부

    private bool isOnConveyor = false;
    private ConveyorBeltPhysics currentConveyorScript = null;

    private PhysicsMaterial2D defaultMaterial; // 기본 마찰
    public PhysicsMaterial2D wallMaterial; // 벽에 붙을시 마찰
    private Collider2D playerCollider;

    public GameObject jumpDustEffect;//먼지 프리펩
    public Transform dustPoint;//이펙트를 생성할 위치

    private bool wasGrounded;//직전 프레임에서 ground 상태 변수

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        defaultMaterial = playerCollider.sharedMaterial;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastDamageTime = -damageCooldown; // 시작 시 무적 상태로 설정
        originalMoveSpeed = moveSpeed; // 원래 속도 저장
    }

    void Update()
    {
        // 점프 입력 처리
        if ((Input.GetButtonDown("Jump") && isGrounded) || (Input.GetButtonDown("Jump") && canJump))
        {
            jumpRequest = true; // 점프 요청
            canJump = false;
            // animator.SetBool("IsJumping", true);
            // animator.SetBool("IsGround", false);
        }
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 스프라이트 방향 전환
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

        if (died && Input.GetKeyDown(KeyCode.R))
        {
            died = false;
            RestartGame();
        }

    }

    void FixedUpdate()
    {
        // 입력이 막혀있는지 확인
        if (Time.time < inputDisabledTime || isTalking || isStackGameMode)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // 움직임 멈춤
            return; // 입력 처리 X
        }
        else
        {
            animator.ResetTrigger("TakeDamage");
        }
        wasGrounded = isGrounded;//직전 Grounded 상태 저장
        // 땅 체크 (FixedUpdate에서 Raycast 사용)
        isGrounded = IsGrounded();

        //착지시 이펙트 발생
        if(!wasGrounded && isGrounded )
        {
            if(jumpRequest != null && dustPoint != null)
            {
                GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                Destroy(dust, 0.2f);
            }
        }

      //  if(isGrounded) Debug.Log("땅");
        // 애니메이션 제어
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsGround", isGrounded);

        // 점프 처리
        if (jumpRequest)
        {
            jumpRequest = false; // 점프 요청 초기화
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            //점프 시 이펙트 생성
            if (jumpDustEffect != null && dustPoint != null)
            {
                GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                Destroy(dust, 0.2f);
            }
        }
        if (!isGrounded && !isTalking && !isStackGameMode)
        {
            //공중에서 속도 줄이기
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y); //
        }

        // 좌우 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        //Vector2 moveVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        //rb.velocity = moveVelocity;
        bool allowPlayerVelocityOverride = true; // 플레이어 입력으로 속도를 덮어쓸지 여부


        if (isOnConveyor && currentConveyorScript != null)
        {
            // 컨베이어와 같은 방향으로 이동 중인지 확인
            if (moveInput != 0 && Mathf.Sign(moveInput) == Mathf.Sign(currentConveyorScript.directionMultiplier))
            {
                allowPlayerVelocityOverride = false;
                float extraPushFactor = 4f; // moveSpeed와 다른 힘 계수, 튜닝 필요
                rb.AddForce(Vector2.right * moveInput * extraPushFactor, ForceMode2D.Force);

            }
        }

        if ((allowPlayerVelocityOverride && Mathf.Abs(moveInput) > 0.01f)) // 입력이 있을 때 (0이 아닐 때)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    bool IsGrounded()
    {
        // 캐릭터 발 밑으로 Raycast를 쏘아서 땅에 닿았는지 확인
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }


    void OnCollisionStay2D(Collision2D collision)
    {
        //벽 붙기 제거
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {

                if (Mathf.Abs(contact.normal.x) > 0.8f)
                {
                    float moveInput = Input.GetAxisRaw("Horizontal");
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
                    isGrounded = IsGrounded();
                    canJump = true;
                    verticalVelocity = 0;
                    Debug.Log("적 밟은 후 isGrounded: " + isGrounded);
                   

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
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Trick")
           || collision.gameObject.CompareTag("Slow") || collision.gameObject.CompareTag("Move"))
        {
            isGrounded = IsGrounded();
            Debug.Log("OnCollisionExit2D 실행");
        }
    }

    public void TakeDamage()
    {
        if (Time.time - lastDamageTime > damageCooldown && !isTakingDamage && !isStackGameMode && !died)
        {
            // 피격 쿨다운 적용
            health -= 10;
            Debug.Log("Player Health: " + health);
            DialogueManager.instance.SetHealth(health);
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

    public void Die()
    {
        died = true;
        Debug.Log("Player Died!");
        GameManager.instance.totalLives--;  // 목숨 하나 감소
        PlayerPrefs.SetInt("TotalLives", GameManager.instance.totalLives);  // PlayerPrefs에 저장
        GameManager.instance.UpdateLifeUI();  // UI 업데이트
        Time.timeScale = 0;
        if (GameManager.instance.totalLives <= 0)
        {
            // 목숨이 0일 경우 게임 오버 처리
            Debug.Log("Game Over");
            game_over = true;
            DialogueManager.instance.SetDiedMessage("Game Over");
            Destroy(gameObject);  // 플레이어 오브젝트 삭제
        }
        else
        {

            GetComponent<Renderer>().enabled = false;
            DialogueManager.instance.SetDiedMessage("oh..만삣삐 넌 할 수 있어 \n\n\n\n\n\n\n\n\n 'R'을 눌러 다시 Stand Up 하는거야!");
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
        Debug.Log("무언가와 충돌 감지됨: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log("코인 트리거 충돌 감지됨!");

            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(1);  // GameManager에서 얻은 코인 개수 1개 증가
            }

            Destroy(collision.gameObject);  // 코인 제거
        }

        if (collision.gameObject.tag == "Finish")
        {
            GameManager.instance.NextStage();
        }

        if (collision.gameObject.CompareTag("Slow")) // 'Slow' 태그를 가진 오브젝트에 닿으면 감속
        {
            ApplySlow(0.5f); // 50% 속도 감소
        }

        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            isOnConveyor = true;
            currentConveyorScript = collision.GetComponent<ConveyorBeltPhysics>(); // 스크립트 이름 확인!
            if (currentConveyorScript == null)
            {
                Debug.LogError("ConveyorBeltPhysics2D 스크립트를 찾을 수 없습니다!", collision.gameObject);
            }
            Debug.Log("Entered Conveyor");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Slow")) // Slow를 벗어나면 속도 원복
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
        moveSpeed = originalMoveSpeed * multiplier;
        Debug.Log("이동 속도 감소: " + moveSpeed);
    }

    public void RemoveSlow()
    {
        moveSpeed = originalMoveSpeed;
        Debug.Log("이동 속도 복구: " + moveSpeed);
    }

    void RestartGame()
    {
        Debug.Log("리셋시작!");
       
        DialogueManager.instance.SetDiedMessage("");
        GameManager.instance.UpdateLifeUI();
        SceneManager.LoadSceneAsync(0).completed += (AsyncOperation operation) =>
        {
            // 씬 로드가 완료된 후 실행되는 코드
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
                // 씬 로드 완료 후 카메라 Confiner 재설정
                //GameManager.instance.GetComponent<GameManager>().Invoke("SetCameraConfinerForCurrentStage", 0.15f);
            }
            GameManager.instance.UpdateLifeUI();
            Time.timeScale = 1; 
        };
    }
    // 스택 게임 전환 시 작동
    public void SetStackGameMode(bool isActive)
    {
        isStackGameMode = isActive;

        if (isActive)
        {
            inputDisabledTime = float.MaxValue; // 입력 차단
            isTakingDamage = true;              // 무적 처리
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGround", true);
        }
        else
        {
            inputDisabledTime = Time.time;      // 입력 재활성화
            isTakingDamage = false;
        }
    }

}