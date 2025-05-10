using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Tooltip("오브젝트 스폰 매니저")]
    public SpawnManager spawnManager;

    [Tooltip("컨베이어 벨트 물리")]
    public ConveyorBeltPhysics conveyorBelt;

    [Tooltip("배경 움직임 (RunningMove)")]
    public RunningMove backgroundMover;

    public GameObject telSpot;

    [Tooltip("시작 지연 시간 (초)")]
    public float startDelay = 2f;
    [Tooltip("작동 지속 시간 (초)")]
    public float activeDuration = 6f;

    private bool isRunning = false; // 기능들이 시작되었는지 확인하는 플래그
    private float startTime; // 시작 시간 기록

    void Start()
    {
        // 지정된 지연 시간 후에 모든 기능 시작
       // Invoke("StartAllFeatures", startDelay);
    }

    public void StartAllFeatures()
    {
        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
            Debug.Log("스폰 매니저 시작");
        }
        else
        {
            Debug.LogWarning("스폰 매니저가 연결되지 않았습니다.");
        }

        if (conveyorBelt != null)
        {
            conveyorBelt.StartConveying();
            Debug.Log("컨베이어 벨트 시작");
        }
        else
        {
            Debug.LogWarning("컨베이어 벨트가 연결되지 않았습니다.");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StartScrolling();
            Debug.Log("배경 움직임 시작");
        }
        else
        {
            Debug.LogWarning("배경 움직임이 연결되지 않았습니다.");
        }

        Debug.Log("모든 기능 시작!");
        isRunning = true;
        startTime = Time.time; // 시작 시간 기록
    }

    void Update()
    {
        if (isRunning)
        {
            // 시작 후 activeDuration 시간이 지났으면 StopAllFeatures 호출
            if (Time.time >= startTime + activeDuration)
            {
                StopAllFeatures();
                isRunning = false; // 더 이상 Update에서 확인하지 않도록 플래그 변경
            }
        }
    }

    // (선택 사항) 필요하다면 모든 기능을 멈추는 함수
    public void StopAllFeatures()
    {
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
            Debug.Log("스폰 매니저 중단");
        }

        if (conveyorBelt != null)
        {
            conveyorBelt.StopConveying();
            Destroy(conveyorBelt.gameObject); // 컨베이어 벨트 오브젝트 자체를 제거
            conveyorBelt = null; // 참조 해제
            Debug.Log("컨베이어 벨트 중단 및 제거");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StopScrolling();
            Debug.Log("배경 움직임 중단");
        }

        if (telSpot != null)
        {
            Destroy(telSpot);
            Debug.Log("텔레포트 지점 제거");
            telSpot = null; // 참조 해제
        }

        Debug.Log("모든 기능 중단!");
    }
}