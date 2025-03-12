using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // 싱글톤 인스턴스
    public int hopeScore = 0;
    //public Text pointTxt;  // 점수 UI 표시용 텍스트
    public int stageIndex;
    public GameObject[] stages = new GameObject[12]; // 크기를 startPositions.Length와 동일하게 설정
    public Transform player;
    public int totalLives = 3;  // 총 목숨 수
    public Image[] lifeImages;  // UI에서 목숨을 나타낼 이미지들
    private Canvas uiCanvas; // canvas 저장 변수
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
            uiCanvas = FindObjectOfType<Canvas>();  // 씬에서 Canvas를 찾아 할당
            if (uiCanvas != null)
            {
                DontDestroyOnLoad(uiCanvas.gameObject);  // Canvas 오브젝트를 씬 전환 시에도 유지
            }
        }
        else
        {
            Destroy(gameObject);
        }

        // PlayerPrefs에서 totalLives 값을 불러오기
        totalLives = PlayerPrefs.GetInt("TotalLives", 3);  // 기본값 3으로 설정
        UpdateLifeUI();  // UI 업데이트
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //코인 5개까지 증가시키는 함수
    public void AddScore(int amount)
    {
        hopeScore += amount;
    }

    // 10초 후 hopeScore를 0으로 초기화하는 코루틴
    IEnumerator ResetScoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hopeScore = 0;
        //UpdateScoreUI();
    }

    

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
            GameManager.instance.hopeScore = 0;
        }
        else
        {
            Debug.LogWarning($"PlayerReposition: stageIndex({stageIndex})가 startPositions 범위를 초과했습니다.");
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
                lifeImages[i].enabled = true;  // 목숨이 있을 때 표시
            }
            else
            {
                lifeImages[i].enabled = false;  // 목숨이 없을 때 숨김
            }
        }
    }

}
