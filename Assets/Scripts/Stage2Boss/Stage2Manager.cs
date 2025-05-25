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

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

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
            ShowGameOver("축하합니다!\n점수 도달로 게임 종료");
            StartCoroutine(RestartAfterDelay(1f));
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

    //public void RestartGame()
    //{
    //    ResetGameState();  // 씬을 리로드하지 않고 직접 초기화
    //}


    public void HandlePlatformStepped(Vector3 currentPlatformPos)
    {

        // UI 시작
        ScoreManager.instance.StartGameUI();

        // 점수 추가
        ScoreManager.instance.AddScore(100);

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

    /*
    private void ResetGameState()
    {
        // 기존 플랫폼 제거
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Platform"))
        {
            if (obj != platform) // 초기 platform 오브젝트는 제외
                Destroy(obj);
        }

        // Stage 하위 오브젝트 비활성화
        if (stageParent != null)
        {
            foreach (Transform child in stageParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Stage 부모 오브젝트 자체도 비활성화 (이 부분 추가!)
        stageParent.SetActive(false);

        // 플레이어 위치 초기화
        if (player != null && playerStartPos != null)
        {
            player.transform.position = playerStartPos.position;
            player.SetActive(true);  // 혹시 비활성화된 경우
        }

        // 카메라 초기화
        if (mainCamera != null)
        {
            mainCamera.SetActive(true);
            AudioListener mainAudio = mainCamera.GetComponent<AudioListener>();
            if (mainAudio != null) mainAudio.enabled = true;
        }

        if (bossCamera != null)
        {
            bossCamera.SetActive(false);
            AudioListener bossAudio = bossCamera.GetComponent<AudioListener>();
            if (bossAudio != null) bossAudio.enabled = false;
        }

        // 게임오버 UI 초기화
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // 점수 초기화
        ScoreManager.instance.ResetScore();

        // BlackWaves 위치 초기화
        if (blackwaves != null && blackwavesStartPos != null)
        {
            blackwaves.transform.position = blackwavesStartPos.position;
            blackwaves.SetActive(true);
        }

        isGameOver = false;
    }
    */
}
