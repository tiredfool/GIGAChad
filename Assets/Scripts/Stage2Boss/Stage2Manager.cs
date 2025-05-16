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

    // 
    public GameObject platformPrefab;
    public GameObject fallingObjectPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;

    public Text gameOverText;
    private bool isGameOver = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
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

        // 카메라 상승 시작
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();
        }

        // 새로운 플랫폼 생성
        float newX = Random.Range(272f, 279f);
        float newY = currentPlatformPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);
        Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);

        // 떨어지는 물체는 랜덤한 딜레이로 생성
        StartCoroutine(SpawnFallingObjectWithDelay(newY));
    }

    IEnumerator SpawnFallingObjectWithDelay(float baseY)
    {
        float delay = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(delay);

        float randomX = Random.Range(260f, 290f);
        Vector3 fallObjectPos = new Vector3(randomX, baseY + 5f, 0f);
        Instantiate(fallingObjectPrefab, fallObjectPos, Quaternion.identity);
    }

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
        if (mainCamera != null) mainCamera.SetActive(true);
        if (bossCamera != null) bossCamera.SetActive(false);

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
}
