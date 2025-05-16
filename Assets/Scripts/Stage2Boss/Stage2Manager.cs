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

        // ī�޶� ��� ����
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();
        }

        // ���ο� �÷��� ����
        float newX = Random.Range(272f, 279f);
        float newY = currentPlatformPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);
        Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);

        // �������� ��ü�� ������ �����̷� ����
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
        if (mainCamera != null) mainCamera.SetActive(true);
        if (bossCamera != null) bossCamera.SetActive(false);

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
}
