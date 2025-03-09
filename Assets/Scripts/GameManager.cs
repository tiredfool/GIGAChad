using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // 싱글톤 인스턴스
    public int hopeScore = 0;
    public int maxHopeScore = 100;
    //public Text pointTxt;  // 점수 UI 표시용 텍스트
    public int stageIndex;
    public GameObject[] stages = new GameObject[12]; // 크기를 startPositions.Length와 동일하게 설정
    public Transform player;
    public Vector3[] startPositions = new Vector3[]
    {
        new Vector3(20, -3, 0),
        new Vector3(53, -3, 0),
        new Vector3(94, -3, 0),
        new Vector3(136, 1, 0),
        new Vector3(178, -3, 0),
        new Vector3(214, 2, 0),

        // 추가된 스테이지 이동 위치
        new Vector3(20, -27, 0),
        new Vector3(71, -24, 0),
        new Vector3(124, -24, 0),
        new Vector3(170, -27, 0),
        new Vector3(201.5f, -22, 0),
        new Vector3(235.5f, -27, 0)
    };


    void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //UpdateScoreUI();  // 초기 점수 UI 업데이트
    }

    void Update()
    {
        // UI가 설정되었을 때만 업데이트
        //if (pointTxt != null)
        //{
        //   pointTxt.text = hopeScore.ToString();
        //}
    }

    // 점수 증가 메서드
    public void AddScore(int amount)
    {
        hopeScore += amount;
        //UpdateScoreUI();

        // hopeScore가 100 이상이 되면 10초 후 초기화
        if (hopeScore >= maxHopeScore)
        {
            StartCoroutine(ResetScoreAfterDelay(10f));
        }
    }

    // 10초 후 hopeScore를 0으로 초기화하는 코루틴
    IEnumerator ResetScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hopeScore = 0;
        //UpdateScoreUI();
    }

    // 점수 UI 업데이트
    /*private void UpdateScoreUI()
    {
        if (pointTxt != null)
        {
            pointTxt.text = hopeScore.ToString();
        }
        else
        {
            Debug.LogWarning("GameManager: pointTxt가 할당되지 않았습니다!");
        }
    }*/

    public void NextStage()
    {
        if (stageIndex < stages.Length - 1 && stageIndex < startPositions.Length - 1) // 초과 방지
        {
            stages[stageIndex].gameObject.SetActive(false);
            stageIndex++;
            stages[stageIndex].gameObject.SetActive(true);
            Debug.Log("Next Stage Index: " + stageIndex);
            PlayerReposition();
        }
        else
        {
            Debug.LogWarning("마지막 스테이지에 도달하여 이동할 수 없습니다.");
        }
    }

    public void PlayerReposition()
    {
        if (stageIndex < startPositions.Length)
        {
            Debug.Log("Player Repositioned to: " + startPositions[stageIndex]);
            player.transform.position = startPositions[stageIndex]; // 스테이지에 맞는 플레이어 위치 설정
        }
        else
        {
            Debug.LogWarning($"PlayerReposition: stageIndex({stageIndex})가 startPositions 범위를 초과했습니다.");
        }
    }


}
