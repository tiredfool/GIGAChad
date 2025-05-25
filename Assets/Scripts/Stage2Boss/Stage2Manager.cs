using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Stage2Manager : MonoBehaviour
{
    public static Stage2Manager instance;

    // �ʱ�ȭ ����
    public GameObject player;          // �÷��̾ ���� ����
    public Transform playerStartPos;   // �÷��̾� �ʱ� ��ġ
    public GameObject stageParent;     // �ν����Ϳ��� Stage ������Ʈ ���� �Ҵ�
    public GameObject platform;
    public Transform platformStartPos;
    public GameObject blackwaves;
    public Transform blackwavesStartPos; // BlackWaves �ʱ� ��ġ
    private bool hasChangedBGM = false;  // BGM ���� ���� üũ

    // 
    public GameObject platformPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;

    public Text gameOverText;
    private bool isGameOver = false;

    // Boss ��ȯ �� ��ֹ� ����
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

        // AudioListener ����
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
            Debug.Log("���� ���� - ���� 5000 ����");
            ShowGameOver("�����մϴ�!\n���� ���޷� ���� ����");
            StartCoroutine(RestartAfterDelay(1f));
        }
    }

    public void EndGameByBlackWave()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("���� ���� - BlackWaves�� ����");
            ShowGameOver("���� ����!\nR Ű�� �����");
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
    //    ResetGameState();  // ���� ���ε����� �ʰ� ���� �ʱ�ȭ
    //}


    public void HandlePlatformStepped(Vector3 currentPlatformPos)
    {

        // UI ����
        ScoreManager.instance.StartGameUI();

        // ���� �߰�
        ScoreManager.instance.AddScore(100);

        // ī�޶� ��ȯ
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // AudioListener ��ȯ
        AudioListener mainAudio = mainCamera?.GetComponent<AudioListener>();
        if (mainAudio != null) mainAudio.enabled = false;
        AudioListener bossAudio = bossCamera?.GetComponent<AudioListener>();
        if (bossAudio != null) bossAudio.enabled = true;

        // Boss_Idle ��Ȱ��ȭ
        if (bossIdle != null) bossIdle.SetActive(false);

        // ī�޶� ��� ����
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();
        }

        // �� ���� BGM ����
        if (!hasChangedBGM)
        {
            MainSoundManager.instance.ChangeBGM("2StageBoss");
            hasChangedBGM = true;
            Debug.Log("BGM�� '2StageBoss'�� ����Ǿ����ϴ�.");
        }

        // Boss �̺�Ʈ�� �� 1���� ����ǵ���
        if (!hasBossEventStarted)
        {
            StartCoroutine(BossRoutine());
        }

        // ���ο� �÷��� ����
        float newX = Random.Range(280f, 287f);
        float newY = currentPlatformPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);
        Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);

        // �������� ��ü�� ������ �����̷� ����
        //StartCoroutine(SpawnFallingObjectWithDelay(newY));
    }

    IEnumerator BossRoutine()
    {
        if (hasBossEventStarted) yield break;
        hasBossEventStarted = true;

        yield return new WaitForSeconds(spawnInterval); // ù ����

        while (true)
        {
            // Boss ���� ��
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

            // Boss ���� ��
            if (bossAttack != null) bossAttack.SetActive(false);
            if (boss != null) boss.SetActive(true);

            isSpawning = false;

            yield return new WaitForSeconds(spawnInterval); // ���� ������� ���
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
        // ���� �÷��� ����
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Platform"))
        {
            if (obj != platform) // �ʱ� platform ������Ʈ�� ����
                Destroy(obj);
        }

        // Stage ���� ������Ʈ ��Ȱ��ȭ
        if (stageParent != null)
        {
            foreach (Transform child in stageParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Stage �θ� ������Ʈ ��ü�� ��Ȱ��ȭ (�� �κ� �߰�!)
        stageParent.SetActive(false);

        // �÷��̾� ��ġ �ʱ�ȭ
        if (player != null && playerStartPos != null)
        {
            player.transform.position = playerStartPos.position;
            player.SetActive(true);  // Ȥ�� ��Ȱ��ȭ�� ���
        }

        // ī�޶� �ʱ�ȭ
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

        // ���ӿ��� UI �ʱ�ȭ
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // ���� �ʱ�ȭ
        ScoreManager.instance.ResetScore();

        // BlackWaves ��ġ �ʱ�ȭ
        if (blackwaves != null && blackwavesStartPos != null)
        {
            blackwaves.transform.position = blackwavesStartPos.position;
            blackwaves.SetActive(true);
        }

        isGameOver = false;
    }
    */
}
