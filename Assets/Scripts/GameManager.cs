using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // �̱��� �ν��Ͻ�
    public int hopeScore = 0;
    public int maxHopeScore = 100;
    //public Text pointTxt;  // ���� UI ǥ�ÿ� �ؽ�Ʈ
    public int stageIndex;
    public GameObject[] stages = new GameObject[12]; // ũ�⸦ startPositions.Length�� �����ϰ� ����
    public Transform player;
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


    void Awake()
    {
        // �̱��� ���� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // �� ��ȯ �� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //UpdateScoreUI();  // �ʱ� ���� UI ������Ʈ
    }

    void Update()
    {
        // UI�� �����Ǿ��� ���� ������Ʈ
        //if (pointTxt != null)
        //{
        //   pointTxt.text = hopeScore.ToString();
        //}
    }

    // ���� ���� �޼���
    public void AddScore(int amount)
    {
        hopeScore += amount;
        //UpdateScoreUI();

        // hopeScore�� 100 �̻��� �Ǹ� 10�� �� �ʱ�ȭ
        if (hopeScore >= maxHopeScore)
        {
            StartCoroutine(ResetScoreAfterDelay(10f));
        }
    }

    // 10�� �� hopeScore�� 0���� �ʱ�ȭ�ϴ� �ڷ�ƾ
    IEnumerator ResetScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hopeScore = 0;
        //UpdateScoreUI();
    }

    // ���� UI ������Ʈ
    /*private void UpdateScoreUI()
    {
        if (pointTxt != null)
        {
            pointTxt.text = hopeScore.ToString();
        }
        else
        {
            Debug.LogWarning("GameManager: pointTxt�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }*/

    public void NextStage()
    {
        if (stageIndex < stages.Length - 1 && stageIndex < startPositions.Length - 1) // �ʰ� ����
        {
            stages[stageIndex].gameObject.SetActive(false);
            stageIndex++;
            stages[stageIndex].gameObject.SetActive(true);
            Debug.Log("Next Stage Index: " + stageIndex);
            PlayerReposition();
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
            Debug.Log("Player Repositioned to: " + startPositions[stageIndex]);
            player.transform.position = startPositions[stageIndex]; // ���������� �´� �÷��̾� ��ġ ����
        }
        else
        {
            Debug.LogWarning($"PlayerReposition: stageIndex({stageIndex})�� startPositions ������ �ʰ��߽��ϴ�.");
        }
    }


}
