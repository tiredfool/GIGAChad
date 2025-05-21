using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : MonoBehaviour
{
    //public float moveSpeed = 5f;

    public float accelerationForce = 50f; // 플레이어 이동 가속도 (새로 추가)
    public float maxMoveSpeed = 5f; // 플레이어 최대 이동 속도 (moveSpeed 대신 사용)

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

    public bool isPlayingFootstepSound = false;
    public float footstepInterval = 0.3f;

    private float horizontalInput; // 런닝머신용
    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        defaultMaterial = playerCollider.sharedMaterial;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastDamageTime = -damageCooldown; // 시작 시 무적 상태로 설정
        originalMoveSpeed = maxMoveSpeed; // 원래 속도 저장
    }

    void Update()
    {
        // 점프 입력 처리
        if ((Input.GetButtonDown("Jump") && isGrounded) || (Input.GetButtonDown("Jump") && canJump))
        {
            jumpRequest = true; // 점프 요청
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
        // 입력이 막혀있는지 확인
        if (Time.time < inputDisabledTime || isTalking || isStackGameMode)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // 움직임 멈춤

            // 이 부분은 FixedUpdate에서 움직임이 멈췄을 때 발소리도 멈추도록 이미 잘 처리되어 있습니다.
            if (isPlayingFootstepSound)
            {
                isPlayingFootstepSound = false;
                StopCoroutine("PlayFootstepSound");
                if (MainSoundManager.instance != null)
                {
                    MainSoundManager.instance.StopSFX("Footstep");
                }
            }

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
        if (!wasGrounded && isGrounded)
        {
            if (jumpRequest != null && dustPoint != null) // jumpRequest는 항상 true/false 부울값이므로 null 비교는 의미 없습니다.
                                                          // 직전 프레임에 점프 요청이 있었는지를 확인하는 것이 더 적절합니다.
                                                          // 여기서는 `wasGrounded`가 false이고 `isGrounded`가 true인 경우에만 이펙트를 생성하는 것으로 충분합니다.
            {
                GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                Destroy(dust, 0.2f);
            }
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Lend");
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

            // 점프 중 발소리 중지
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
            //공중에서 속도 줄이기
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y); //
        }

        // 좌우 이동
        //float moveInput = Input.GetAxisRaw("Horizontal");
        //bool allowPlayerVelocityOverride = true; // 플레이어 입력으로 속도를 덮어쓸지 여부


        //if (isOnConveyor && currentConveyorScript != null)
        //{
        //    // 컨베이어와 같은 방향으로 이동 중인지 확인
        //    if (moveInput != 0 && Mathf.Sign(moveInput) == Mathf.Sign(currentConveyorScript.directionMultiplier))
        //    {
        //        allowPlayerVelocityOverride = false;
        //        float extraPushFactor = 4f; // moveSpeed와 다른 힘 계수, 튜닝 필요
        //        rb.AddForce(Vector2.right * moveInput * extraPushFactor, ForceMode2D.Force);

        //    }
        //}

        //if ((allowPlayerVelocityOverride && Mathf.Abs(moveInput) > 0.01f)) // 입력이 있을 때 (0이 아닐 때)
        //{
        //    rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        //}
        //else if (allowPlayerVelocityOverride && Mathf.Abs(moveInput) <= 0.01f) // 입력이 없을 때 (추가: 멈출 때 발소리 끔)
        //{
        //    // 입력이 없으면 캐릭터가 멈추고 발소리도 멈춰야 합니다.
        //    // 단, 컨베이어 벨트 위에 있거나 다른 외부 힘에 의해 움직이는 경우는 예외로 둬야 할 수도 있습니다.
        //    // 여기서는 단순히 입력이 없으면 수평 속도를 0으로 만듭니다.
        //    rb.velocity = new Vector2(0, rb.velocity.y);
        //}


        //animator.SetFloat("Speed", Mathf.Abs(moveInput));

        //// 발소리 재생 조건 강화: 움직임 입력이 0.1보다 커야 하고, 땅에 닿아 있어야 하며, 대화 중이 아니고, 스택 게임 모드가 아닐 때
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
            // 플레이어의 현재 수평 속도가 최대 속도 범위 내에 있는지 확인
            // 그리고 목표 방향으로의 가속이 가능한지 확인
            if (Mathf.Abs(rb.velocity.x) < maxMoveSpeed || Mathf.Sign(rb.velocity.x) != Mathf.Sign(horizontalInput))
            {
                rb.AddForce(Vector2.right * horizontalInput * accelerationForce, ForceMode2D.Force);
            }
            // 입력이 없으면 감속 (선택 사항, 필요에 따라 주석 처리)
            else if (horizontalInput == 0 && isGrounded && !isOnConveyor)
            {
                // 땅에 있을 때만 수평 속도를 서서히 0으로
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.fixedDeltaTime * 10f), rb.velocity.y);
            }
        }
        //else
        //{
        //    // 입력이 비활성화된 경우, 플레이어의 수평 속도를 0으로
        //    // (컨베이어 벨트의 영향을 받지 않도록)
        //    rb.velocity = new Vector2(0, rb.velocity.y);
        //}

        // 컨베이어 벨트 힘 적용
        if (isOnConveyor && currentConveyorScript != null)
        {
            Vector2 conveyorPushDirection = currentConveyorScript.transform.right * currentConveyorScript.directionMultiplier;
            rb.AddForce(conveyorPushDirection * currentConveyorScript.pushForce, ForceMode2D.Force);
        }

      
        //// 만약 공중에서 빠르게 멈추는 느낌을 원하면 유지
        // if (!isGrounded && !isTalking && !isStackGameMode)
        // {
        //     rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
        // }

        // 애니메이션 제어
        //animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x)); // 실제 속도 사용
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        // 발소리 재생 조건 (actual velocity 사용)
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
                    // float moveInput = Input.GetAxisRaw("Horizontal"); // 여기서 moveInput을 사용하지 않으므로 제거해도 무방합니다.
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
                    isGrounded = IsGrounded(); // 밟은 후 다시 땅 체크
                    canJump = true;
                    verticalVelocity = 0; // verticalVelocity는 사용되지 않는 변수 같습니다.
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
        MainSoundManager.instance.StopAllSFX();
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

        // 대화 시작/종료 시 발소리 코루틴 중지
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
        maxMoveSpeed = originalMoveSpeed * multiplier;
        Debug.Log("이동 속도 감소: " + maxMoveSpeed);
    }

    public void RemoveSlow()
    {
        maxMoveSpeed = originalMoveSpeed;
        Debug.Log("이동 속도 복구: " + maxMoveSpeed);
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
            // 스택 게임 모드 진입 시 발소리 중지
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
            inputDisabledTime = Time.time;      // 입력 재활성화
            isTakingDamage = false;
        }
    }

    IEnumerator PlayFootstepSound() // 걸음소리 
    {
        // 코루틴 내부에서 지속적으로 현재 입력 상태를 확인합니다.
        while (isGrounded && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f && !isTalking && !isStackGameMode)
        {
            // SoundManager가 존재하고 Footstep 사운드 재생 함수가 있을 경우 호출
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Footstep"); // "Footstep"은 사운드 클립의 이름 또는 식별자입니다.
            }
            yield return new WaitForSeconds(footstepInterval);
        }
        // while 루프 조건이 false가 되어 코루틴이 종료될 때,
        // isPlayingFootstepSound 상태를 false로 업데이트하고 사운드를 중지합니다.
        isPlayingFootstepSound = false;
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.StopSFX("Footstep");
        }
    }
}