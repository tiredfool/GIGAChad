using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Stage2Manager : MonoBehaviour
{
    public static Stage2Manager instance;

    // 초기화 관련
    public GameObject player;          // 플레이어를 직접 지정
    public Transform playerStartPos;   // 플레이어 초기 위치
    public GameObject stageParent;     // 인스펙터에서 Stage 오브젝트 직접 할당
    public GameObject platform;
    public Transform platformStartPos;
    public GameObject blackwaves;
    public Transform blackwavesStartPos; // BlackWaves 초기 위치
    private bool hasChangedBGM = false;  // BGM 변경 여부 체크

    // 
    public GameObject platformPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;

    public Text gameOverText;
    private bool isGameOver = false;
    public GameObject uiCanvasToHide;

    // Boss 소환 및 장애물 관련
    public GameObject boss;
    public GameObject bossAttack;
    public GameObject bossIdle;
    public GameObject fallingObjectPrefab;

    public float spawnInterval = 5f;
    public float bossAttackDuration = 3f;
    public float spawnRate = 0.7f;

    private bool isSpawning = false;
    private bool hasBossEventStarted = false;

    private StackManager stackManager; // StackManager 참조를 위한 변수 추가

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        stackManager = FindObjectOfType<StackManager>();
        if (stackManager == null)
        {
            Debug.LogError("Stage2Manager: 씬에서 StackManager를 찾을 수 없습니다. 다이얼로그 가시성 설정에 문제가 발생할 수 있습니다.");
        }

        // GameManager 인스턴스 존재 여부 확인
        if (GameManager.instance == null) // 'instance'로 접근
        {
            Debug.LogWarning("Stage2Manager: GameManager.instance를 찾을 수 없습니다. 게임 진행 상태가 올바르지 않을 수 있습니다.");
        }

        if (boss != null) boss.SetActive(true);
        if (bossAttack != null) bossAttack.SetActive(false);
        if (bossIdle != null) bossIdle.SetActive(true);

        // AudioListener 끄기
        if (bossCamera != null)
        {
            AudioListener bossAudio = bossCamera.GetComponent<AudioListener>();
            if (bossAudio != null) bossAudio.enabled = false;
        }
    }

    void Update()
    {

    }

    public void EndGameByScore()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("게임 종료 - 점수 5000 도달");
            if (uiCanvasToHide != null)
            {
                uiCanvasToHide.SetActive(false);
                Debug.Log($"{uiCanvasToHide.name} Canvas가 비활성화되었습니다.");
            }
            MainSoundManager.instance.StopBGM();
            MainSoundManager.instance.PlayBGM("Hope");
            DialogueManager.instance.StartDialogueByIdRange("E-s", "E-2e");
        }
    }

    public void EndGameByBlackWave()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("게임 종료 - BlackWaves에 닿음");
            ShowGameOver("게임 오버!\nR 키로 재시작");
        }
    }

    private void ShowGameOver(string message)
    {
        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);
        }
    }

    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void HandlePlatformStepped(Vector3 currentPlatformPos)
    {

        // UI 시작
        ScoreManager.instance.StartGameUI();

        // 점수 추가 로직: GameManager의 HasStackGameSucceededThisRun 상태 확인
        if (GameManager.instance != null && GameManager.instance.HasStackGameSucceededThisRun) // 'instance'로 접근
        {
            ScoreManager.instance.AddScore(150); // 현재 게임 세션에서 스택 게임 성공 시 150점
            Debug.Log("Platform Stepped: 현재 게임 세션에서 스택 게임 성공 상태이므로 150점 추가.");
        }
        else
        {
            ScoreManager.instance.AddScore(100); // 현재 게임 세션에서 스택 게임 미성공 상태이므로 100점
            Debug.Log("Platform Stepped: 현재 게임 세션에서 스택 게임 미성공 상태이므로 100점 추가.");
        }

        /*
        // 점수 추가
        ScoreManager.instance.AddScore(100);
        */

        // 카메라 전환
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // AudioListener 전환
        AudioListener mainAudio = mainCamera?.GetComponent<AudioListener>();
        if (mainAudio != null) mainAudio.enabled = false;
        AudioListener bossAudio = bossCamera?.GetComponent<AudioListener>();
        if (bossAudio != null) bossAudio.enabled = true;

        // Boss_Idle 비활성화
        if (bossIdle != null) bossIdle.SetActive(false);

        // 카메라 상승 시작
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();
        }

        // 한 번만 BGM 변경
        if (!hasChangedBGM)
        {
            MainSoundManager.instance.ChangeBGM("2StageBoss");
            hasChangedBGM = true;
            Debug.Log("BGM이 '2StageBoss'로 변경되었습니다.");
        }

        // Boss 이벤트는 단 1번만 실행되도록
        if (!hasBossEventStarted)
        {
            StartCoroutine(BossRoutine());
        }

        // 새로운 플랫폼 생성
        float newX = Random.Range(280f, 287f);
        float newY = currentPlatformPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);
        Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);

        // 떨어지는 물체는 랜덤한 딜레이로 생성
        //StartCoroutine(SpawnFallingObjectWithDelay(newY));
    }

    IEnumerator BossRoutine()
    {
        if (hasBossEventStarted) yield break;
        hasBossEventStarted = true;

        yield return new WaitForSeconds(spawnInterval); // 첫 지연

        while (true)
        {
            // Boss 공격 시
            if (boss != null) boss.SetActive(false);
            if (bossAttack != null && boss != null)
            {
                bossAttack.transform.position = boss.transform.position;
                bossAttack.SetActive(true);
            }
            /*
            if (bossAttack != null)
            {
                bossAttack.SetActive(true);
                Vector3 pos = bossAttack.transform.position;
                bossAttack.transform.position = new Vector3(pos.x, player.transform.position.y + 0f, pos.z);
            }
            */

            isSpawning = true;
            yield return StartCoroutine(SpawnFallingObjects());

            // Boss 공격 끝
            if (bossAttack != null) bossAttack.SetActive(false);
            if (boss != null) boss.SetActive(true);

            isSpawning = false;

            yield return new WaitForSeconds(spawnInterval); // 다음 등장까지 대기
        }
    }

    IEnumerator SpawnFallingObjects()
    {
        float elapsed = 0f;

        while (elapsed < bossAttackDuration)
        {
            float randomX = Random.Range(278f, 291f);
            float spawnY = player.transform.position.y + 7f;

            Instantiate(fallingObjectPrefab, new Vector3(randomX, spawnY, 0f), Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);

            elapsed += spawnRate;
        }
    }
}
