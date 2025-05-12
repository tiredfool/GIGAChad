using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Cinemachine; // Cinemachine 네임스페이스 추가

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // 싱글톤 인스턴스
    public int hopeScore = 0;
    //public Text pointTxt;  // 점수 UI 표시용 텍스트
    public int stageIndex = 0; // 초기 스테이지 인덱스 0으로 설정
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

    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera; // Inspector에서 Virtual Camera 할당
    public CinemachineConfiner2D confiner2D; // Inspector에서 Confiner2D 할당
    private Vector3 originalDamping;

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

        // Virtual Camera가 할당되었다면 초기 댐핑 값 저장
        if (virtualCamera != null && virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
        {
            CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
            originalDamping = new Vector3(transposer.m_XDamping, transposer.m_YDamping, transposer.m_ZDamping);
        }
    }

    void Start()
    {
        player.transform.position = startPositions[stageIndex];
        // 초기 스테이지 활성화 및 카메라 범위 설정
        if (stages.Length > 0 && stageIndex < stages.Length)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i].SetActive(i == stageIndex);
            }
            SetCameraConfinerBounds(stages[stageIndex]);
        }
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
            // 맵 이동 전에 댐핑 0으로 설정
            SetCameraDamping(Vector3.zero);

            stages[stageIndex].gameObject.SetActive(false);
            stageIndex++;
            stages[stageIndex].gameObject.SetActive(true);
            Debug.Log("Next Stage Index: " + stageIndex);
            PlayerReposition();
            SetCameraConfinerBounds(stages[stageIndex]); // 다음 스테이지의 콜라이더로 범위 설정

            // 딜레이 후 댐핑 원래 값으로 복원
            StartCoroutine(ResetCameraDampingAfterDelay(0.1f)); // 0.1초 딜레이 후 복원
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
            // 맵 이동 전에 댐핑 0으로 설정
            SetCameraDamping(Vector3.zero);

            Debug.Log("Player Repositioned to: " + startPositions[stageIndex]);
            player.transform.position = startPositions[stageIndex]; // 스테이지에 맞는 플레이어 위치 설정
            GameManager.instance.hopeScore = 0;

            // 딜레이 후 댐핑 원래 값으로 복원
            StartCoroutine(ResetCameraDampingAfterDelay(0.1f)); // 0.1초 딜레이 후 복원
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

    // 카메라 댐핑 설정 함수
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

    // 딜레이 후 카메라 댐핑 원래 값으로 복원하는 코루틴
    IEnumerator ResetCameraDampingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCameraDamping(originalDamping);
    }

    // CinemachineConfiner2D의 Bounding Volume 2D를 설정하는 함수
    private void SetCameraConfinerBounds(GameObject stage)
    {
        if (confiner2D != null && stage != null)
        {
            Collider2D stageCollider = stage.GetComponent<Collider2D>();
            if (stageCollider != null)
            {
                confiner2D.m_BoundingShape2D = stageCollider;
                Debug.Log($"카메라 범위를 스테이지 '{stage.name}'의 콜라이더로 설정했습니다.");
            }
            else
            {
                Debug.LogError(stage.name + "에 Collider2D 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("CinemachineConfiner2D 또는 스테이지가 할당되지 않았습니다!");
        }
    }

}