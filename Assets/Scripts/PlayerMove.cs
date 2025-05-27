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
    public float someHorizontalKnockbackForce = 1f; //수평 넉백 힘
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
    private float verticalVelocity; // 수직 속도 저장 변수 (현재 코드에서 사용되지 않음)
    private bool canJump = false; // 공중 점프 허용 여부

    public FollowPlayer follower; // 따라오는 기가차드

    private bool isStackGameMode = false; // 스택 게임 모드 여부

    private bool isOnConveyor = false;
    private ConveyorBeltPhysics currentConveyorScript = null;

    private PhysicsMaterial2D defaultMaterial; // 기본 마찰
    public PhysicsMaterial2D wallMaterial; // 벽에 붙을시 마찰
    private Collider2D playerCollider;

    public GameObject jumpDustEffect; //먼지 프리펩
    public Transform dustPoint; //이펙트를 생성할 위치

    private bool wasGrounded; //직전 프레임에서 ground 상태 변수

    public bool isPlayingFootstepSound = false;
    public float footstepInterval = 0.3f;

    private float horizontalInput; // *** 이 변수를 조이스틱/키보드 입력에 따라 업데이트할 것입니다. ***
    private int currentHorizontalDirection = 0; // -1: 왼쪽, 0: 없음, 1: 오른쪽

    // --- 추가된 변수 ---
    private float fixedYPosition; // Y 고정 시 사용할 Y 위치
    private bool wasPlayerInteractionEnabled = true; // 이전 프레임의 플레이어 상호작용 상태
    // ---
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
        // GameManager 인스턴스가 없는 경우 (씬 로드 직후 등) 예외 처리
        if (GameManager.instance == null) return;

        // 점프 입력 처리
        if ((VirtualInputManager.Instance.GetKeyOrButtonDown("Jump") && isGrounded) || (VirtualInputManager.Instance.GetKeyOrButtonDown("Jump") && canJump))
        {
            // GameManager에서 플레이어 상호작용이 비활성화되어 있으면 점프 불가
            if (!GameManager.instance.IsPlayerInteractionEnabled()) return;

            jumpRequest = true; // 점프 요청
            canJump = false;
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Jump");
            }
        }

        // 이동 입력 감지 (FixedUpdate에서 사용하기 위해 저장)
        // horizontalInput = Input.GetAxisRaw("Horizontal"); // 기존 Input.GetAxisRaw 주석 처리 또는 삭제

        // VirtualInputManager를 통해 현재 수평 이동 방향을 가져옵니다.
        currentHorizontalDirection = 0;
        if (VirtualInputManager.Instance.GetKeyOrButton("Left"))
        {
            currentHorizontalDirection = -1;
           // Debug.Log("[PlayerController] Left Input Detected!"); // 추가
        }
        else if (VirtualInputManager.Instance.GetKeyOrButton("Right"))
        {
            currentHorizontalDirection = 1;
          //  //Debug.Log("[PlayerController] Right Input Detected!"); // 추가
        }

        horizontalInput = currentHorizontalDirection;
        ////Debug.Log($"[PlayerController] currentHorizontalDirection: {currentHorizontalDirection}, horizontalInput: {horizontalInput}"); // 추가


        // 스프라이트 방향 전환
        if (horizontalInput > 0) // currentHorizontalDirection 대신 horizontalInput 사용
        {
            spriteRenderer.flipX = false;
            follower.SetNegativeDistance(false);
        }
        else if (horizontalInput < 0) // currentHorizontalDirection 대신 horizontalInput 사용
        {
            spriteRenderer.flipX = true;
            follower.SetNegativeDistance(true);
        }

        // 재시작 처리 (죽었을 때)
        if (died && VirtualInputManager.Instance.GetKeyOrButtonDown("Action") && DialogueManager.instance != null && !DialogueManager.instance.isTyping) // "Action" 가상 키 사용
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
            else // 그 외의 입력 비활성화
            {
                if (wasPlayerInteractionEnabled && !currentPlayerInteractionEnabled)
                {
                    fixedYPosition = transform.position.y;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                    //Debug.Log($"[PlayerController] Y 위치 고정 시작. 고정 Y: {fixedYPosition}");
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
            // 애니메이션 속도를 0으로 설정하여 정지 상태 유지
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsJumping", false); // 점프 애니메이션도 해제
            animator.SetBool("IsGround", true); // 땅 애니메이션 강제

            // TakeDamage 애니메이션 트리거 초기화 (입력 불가 시)
            animator.ResetTrigger("TakeDamage");

            wasPlayerInteractionEnabled = currentPlayerInteractionEnabled; // 상태 업데이트
            return;
        }
        else
        {
            // 움직임이 다시 활성화되는 순간 Y 위치 고정 해제
            if (!wasPlayerInteractionEnabled && currentPlayerInteractionEnabled) // 방금 활성화 상태가 된 경우
            {
                rb.gravityScale = 1f; // 중력 다시 적용
                //Debug.Log("[PlayerController] Y 위치 고정 해제 및 중력 복원.");
            }

            wasPlayerInteractionEnabled = currentPlayerInteractionEnabled; // 상태 업데이트
            wasGrounded = isGrounded; // 직전 Grounded 상태 저장
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

            // 애니메이션 제어 (점프/땅)
            animator.SetBool("IsJumping", !isGrounded);
            animator.SetBool("IsGround", isGrounded);

            // 점프 처리
            if (jumpRequest)
            {
                jumpRequest = false; // 점프 요청 초기화
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);

                // 점프 시 이펙트 생성
                if (jumpDustEffect != null && dustPoint != null)
                {
                    GameObject dust = Instantiate(jumpDustEffect, dustPoint.position, Quaternion.identity);
                    Destroy(dust, 0.2f);
                }

                // 점프 중 발소리 중지 (이미 잘 처리되어 있지만, 재확인)
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

            // 공중에서 속도 줄이기 (현재 코드 유지)
            if (!isGrounded && !isTalking && !isStackGameMode) // isTalking과 isStackGameMode는 shouldDisableMovement에서 이미 처리됨
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            }

            // 플레이어 이동 (가속도 기반)
            if (horizontalInput != 0) // 입력이 있을 때 (이제 horizontalInput은 -1, 0, 1 값을 가짐)
            {
                // 플레이어의 현재 수평 속도가 최대 속도 범위 내에 있는지 확인
                // 그리고 목표 방향으로의 가속이 가능한지 확인
                if (Mathf.Abs(rb.velocity.x) < maxMoveSpeed || Mathf.Sign(rb.velocity.x) != Mathf.Sign(horizontalInput))
                {
                    rb.AddForce(Vector2.right * horizontalInput * accelerationForce, ForceMode2D.Force);
                }
            }
            else if (isGrounded && !isOnConveyor) // 입력이 없고, 땅에 있고, 컨베이어 위가 아닐 때
            {
                // 땅에 있을 때만 수평 속도를 서서히 0으로 (감속)
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.fixedDeltaTime * 10f), rb.velocity.y);
            }

            // 컨베이어 벨트 힘 적용
            if (isOnConveyor && currentConveyorScript != null)
            {
                Vector2 conveyorPushDirection = currentConveyorScript.transform.right * currentConveyorScript.directionMultiplier;
                rb.AddForce(conveyorPushDirection * currentConveyorScript.pushForce, ForceMode2D.Force);
            }

            // 애니메이션 제어 (이동 속도)
            // animator.SetFloat("Speed", Mathf.Abs(horizontalInput)); // 입력 값 기반 (원래 코드 유지)
            // 애니메이션 Speed는 실제 이동 속도에 맞춰주는 것이 더 자연스럽습니다.
            // 조이스틱/키보드 입력이 있을 때 animator.SetFloat("Speed", 1f) (움직임); 없을 때 0f (정지)
            // 또는 Mathf.Abs(rb.velocity.x)를 사용
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x) > 0.1f ? 1f : 0f); // 변경 제안

            // 발소리 재생 조건 (수정: 실제 속도 대신 입력 사용, 이미 위에 있음)
            bool shouldPlayFootsteps = isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f; // 여기서 rb.velocity.x를 사용하는 것이 더 정확할 수 있습니다.

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
            //Debug.Log("플레이어가 맞았습니다!");

            if (rb.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                //Debug.Log("밟기");
                Enemy_Move enemy = collision.gameObject.GetComponent<Enemy_Move>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);
                    rb.velocity = new Vector2(rb.velocity.x, 3f);
                    isGrounded = IsGrounded();
                    canJump = true;
                    verticalVelocity = 0;
                    //Debug.Log("적 밟은 후 isGrounded: " + isGrounded);
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

            // 수평 넉백 방향 계산 (적에게서 멀어지는 방향)
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

            rb.AddForce(Vector2.up * knockbackVerticalSpeed, ForceMode2D.Impulse); // 수직 넉백만 적용

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
            if (DialogueManager.instance != null) // DialogueManager 인스턴스 확인
            {
                DialogueManager.instance.SetDiedMessage("Game Over");
            }
            Destroy(gameObject);
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            if (DialogueManager.instance != null) // DialogueManager 인스턴스 확인
            {
                DialogueManager.instance.SetDiedMessage("oh..만삣삐 넌 할 수 있어 \n\n\n\n\n\n\n\n\n 'R'을 눌러 다시 Stand Up 하는거야!");
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
        //Debug.Log("무언가와 충돌 감지됨: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Coin"))
        {
            //Debug.Log("코인 트리거 충돌 감지됨!");

            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(1);
            }

            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Coin");
                //Debug.Log("코인 사운드 재생");
            }

            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Finish")
        {
            if (GameManager.instance != null) // GameManager 인스턴스 확인
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
                //Debug.LogError("ConveyorBeltPhysics 스크립트를 찾을 수 없습니다!", collision.gameObject);
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
        //Debug.Log("이동 속도 감소: " + maxMoveSpeed);
    }

    public void RemoveSlow()
    {
        maxMoveSpeed = originalMoveSpeed;
        //Debug.Log("이동 속도 복구: " + maxMoveSpeed);
    }

    void RestartGame()
    {
        //Debug.Log("리셋 시작!");


        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.setBlack();
            //Debug.Log("화면을 검게 설정했습니다.");
        }
        else
        {
            //Debug.LogError("DialogueManager 인스턴스를 찾을 수 없습니다! 화면을 검게 만들 수 없습니다.");

        }


        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateLifeUI();
        }


        SceneManager.LoadSceneAsync("SampleScene").completed += (AsyncOperation operation) =>
        {
            //Debug.Log("씬 로드 완료: " + operation.isDone);


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
                        DialogueManager.instance.SetHealth(health); // DialogueManager의 체력 UI 업데이트
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
                //Debug.Log("화면을 밝게 전환 시작했습니다.");
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
        // 걷는 소리는 플레이어가 실제로 움직일 때만 재생 (rb.velocity.x 사용)
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
            // 움직임 활성화 시 애니메이터 초기화
            animator.ResetTrigger("TakeDamage");
        }
    }
}