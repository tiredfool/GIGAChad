using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Cinemachine; // Cinemachine ���ӽ����̽� �߰�

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // �̱��� �ν��Ͻ�
    public int hopeScore = 0;
    //public Text pointTxt;  // ���� UI ǥ�ÿ� �ؽ�Ʈ
    public int stageIndex = 0; // �ʱ� �������� �ε��� 0���� ����
    public GameObject[] stages = new GameObject[12]; // ũ�⸦ startPositions.Length�� �����ϰ� ����
    public Transform player;
    public int totalLives = 3;  // �� ��� ��
    public Image[] lifeImages;  // UI���� ����� ��Ÿ�� �̹�����
    private Canvas uiCanvas; // canvas ���� ����
    public Vector3[] startPositions = new Vector3[]
    {
        new Vector3(20, -3, 0),
        new Vector3(53, -3, 0),
        new Vector3(94, -3, 0),
        new Vector3(136, 1, 0),
        new Vector3(178, -3, 0),
        new Vector3(214, 2, 0),

        // �߰��� �������� �̵� ��ġ
        new Vector3(20, -27, 0),
        new Vector3(71, -24, 0),
        new Vector3(124, -24, 0),
        new Vector3(170, -27, 0),
        new Vector3(201.5f, -22, 0),
        new Vector3(235.5f, -27, 0)
    };

    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera; // Inspector���� Virtual Camera �Ҵ�
    public CinemachineConfiner2D confiner2D; // Inspector���� Confiner2D �Ҵ�
    private Vector3 originalDamping;

    void Awake()
    {
        
        // �̱��� ���� ����
        if (instance == null)
        {
            PlayerPrefs.DeleteKey("TotalLives");
            instance = this;
            DontDestroyOnLoad(gameObject);  // �� ��ȯ �� ����
            uiCanvas = FindObjectOfType<Canvas>();  // ������ Canvas�� ã�� �Ҵ�
            if (uiCanvas != null)
            {
                DontDestroyOnLoad(uiCanvas.gameObject);  // Canvas ������Ʈ�� �� ��ȯ �ÿ��� ����
            }
        }
        else
        {
            Destroy(gameObject);
        }

        // PlayerPrefs���� totalLives ���� �ҷ�����
        

        // Virtual Camera�� �Ҵ�Ǿ��ٸ� �ʱ� ���� �� ����
        if (virtualCamera != null && virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
        {
            CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
            originalDamping = new Vector3(transposer.m_XDamping, transposer.m_YDamping, transposer.m_ZDamping);
        }
    }

    void Start()
    {
       
        totalLives = PlayerPrefs.GetInt("TotalLives", 3);  // �⺻�� 3���� ����
        UpdateLifeUI();  // UI ������Ʈ

        FindAndSetStagesByParent();
        //Debug.Log("GameManager Start - totalLives: " + totalLives);
        player.transform.position = startPositions[stageIndex];
        // �ʱ� �������� Ȱ��ȭ �� ī�޶� ���� ����
        if (stages.Length > 0 && stageIndex < stages.Length)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].SetActive(i == stageIndex);
            }
            SetCameraConfinerBounds(stages[stageIndex]);
        }
    }



    public void FindPlayer()
    {
        // "Player" �±׸� ���� ������Ʈ�� ã�� player ������ �Ҵ�
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            // Player ������Ʈ���� Cinemachine Virtual Camera ������Ʈ ã�� (�ڽ� ������Ʈ���� ã�� ���� ����)
            virtualCamera = playerObject.GetComponentInChildren<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("Player ������Ʈ �Ǵ� �� �ڽĿ��� CinemachineVirtualCamera ������Ʈ�� ã�� �� �����ϴ�!");
            }

            // Player ������Ʈ���� Cinemachine Confiner 2D ������Ʈ ã�� (�ڽ� ������Ʈ���� ã�� ���� ����)
            confiner2D = playerObject.GetComponentInChildren<CinemachineConfiner2D>();
            if (confiner2D == null)
            {
                Debug.LogError("Player ������Ʈ �Ǵ� �� �ڽĿ��� CinemachineConfiner2D ������Ʈ�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError("Player �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }



    public void FindAndSetStagesByParent()
    {
        GameObject stagesParent = GameObject.Find("Stage");
        if (stagesParent != null)
        {
            stages = new GameObject[stagesParent.transform.childCount];
            for (int i = 0; i < stagesParent.transform.childCount; i++)
            {
                stages[i] = stagesParent.transform.GetChild(i).gameObject;
            }
            // ���������� ã�� �� ī�޶� Confiner ���� �õ�
            SetCameraConfinerForCurrentStage();
        }
        else
        {
            stages = new GameObject[0];
        }
    }

    public void ResetStageActivation() // ����۽� Ȱ��
    {
        if (stages.Length > 0 && stageIndex < stages.Length)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                if (stages[i] != null)
                {
                    stages[i].SetActive(i == stageIndex);
                }
            }
            SetCameraConfinerBounds(stages[stageIndex]); // ����� �� ����
        }
    }

    
    //���� 5������ ������Ű�� �Լ�
    public void AddScore(int amount)
    {
        hopeScore += amount;
    }

    // 10�� �� hopeScore�� 0���� �ʱ�ȭ�ϴ� �ڷ�ƾ
    IEnumerator ResetScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hopeScore = 0;
        //UpdateScoreUI();
    }


    public void NextStage()
    {
        if (stageIndex < stages.Length - 1 && stageIndex < startPositions.Length - 1) // �ʰ� ����
        {
            // �� �̵� ���� ���� 0���� ����
            SetCameraDamping(Vector3.zero);

            stages[stageIndex].gameObject.SetActive(false);
            stageIndex++;
            stages[stageIndex].gameObject.SetActive(true);
            Debug.Log("Next Stage Index: " + stageIndex);
            PlayerReposition();
            SetCameraConfinerBounds(stages[stageIndex]); // ���� ���������� �ݶ��̴��� ���� ����

            // ������ �� ���� ���� ������ ����
            StartCoroutine(ResetCameraDampingAfterDelay(0.1f)); // 0.1�� ������ �� ����
        }
        else
        {
            Debug.LogWarning("������ ���������� �����Ͽ� �̵��� �� �����ϴ�.");
        }
    }

    public void PlayerReposition()
    {
        if (stageIndex < startPositions.Length)
        {
            // �� �̵� ���� ���� 0���� ����
            SetCameraDamping(Vector3.zero);

            Debug.Log("Player Repositioned to: " + startPositions[stageIndex]);
            player.transform.position = startPositions[stageIndex]; // ���������� �´� �÷��̾� ��ġ ����
            GameManager.instance.hopeScore = 0;

            // ������ �� ���� ���� ������ ����
            StartCoroutine(ResetCameraDampingAfterDelay(0.1f)); // 0.1�� ������ �� ����
        }
        else
        {
            Debug.LogWarning($"PlayerReposition: stageIndex({stageIndex})�� startPositions ������ �ʰ��߽��ϴ�.");
        }
    }

    public int getScore()
    {
        return hopeScore;
    }

    public void UpdateLifeUI()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < totalLives)
            {
                lifeImages[i].enabled = true;  // ����� ���� �� ǥ��
            }
            else
            {
                lifeImages[i].enabled = false;  // ����� ���� �� ����
            }
        }

    }

    // ī�޶� ���� ���� �Լ�
    private void SetCameraDamping(Vector3 dampingValues)
    {
        if (virtualCamera != null && virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
        {
            CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
            transposer.m_XDamping = dampingValues.x;
            transposer.m_YDamping = dampingValues.y;
            transposer.m_ZDamping = dampingValues.z;
        }
    }

    // ������ �� ī�޶� ���� ���� ������ �����ϴ� �ڷ�ƾ
    IEnumerator ResetCameraDampingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCameraDamping(originalDamping);
    }

    // CinemachineConfiner2D�� Bounding Volume 2D�� �����ϴ� �Լ�
    private void SetCameraConfinerBounds(GameObject stage)
    {
        if (confiner2D != null && stage != null)
        {
            Collider2D stageCollider = stage.GetComponent<Collider2D>();
            if (stageCollider != null)
            {
                confiner2D.m_BoundingShape2D = stageCollider;
                Debug.Log($"ī�޶� ������ �������� '{stage.name}'�� �ݶ��̴��� �����߽��ϴ�.");
            }
            else
            {
                Debug.LogError(stage.name + "�� Collider2D ������Ʈ�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError("CinemachineConfiner2D �Ǵ� ���������� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }
    public void SetCameraConfinerForCurrentStage()
    {
        if (confiner2D != null && stages.Length > 0 && stageIndex < stages.Length && stages[stageIndex] != null)
        {
            Collider2D stageCollider = stages[stageIndex].GetComponent<Collider2D>();
            if (stageCollider != null)
            {
                confiner2D.m_BoundingShape2D = stageCollider;
                Debug.Log($"ī�޶� ������ �������� '{stages[stageIndex].name}'�� �ݶ��̴��� �缳���߽��ϴ�.");
            }
            else
            {
                Debug.LogError(stages[stageIndex].name + "�� Collider2D ������Ʈ�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogWarning("CinemachineConfiner2D, stages �迭, �Ǵ� ���� ���������� ��ȿ���� �ʾ� ī�޶� ������ ������ �� �����ϴ�.");
        }
    }


}