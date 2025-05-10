using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Tooltip("스폰할 오브젝트")]
    public GameObject objectToSpawn;

    [Tooltip("스폰 범위의 중심 위치")]
    public Vector3 spawnCenter = Vector3.zero;

    [Tooltip("스폰 범위의 X축 절반 길이")]
    public float spawnRangeX = 5f;

    [Tooltip("스폰 범위의 Y축 절반 길이")]
    public float spawnRangeY = 3f;

    [Tooltip("스폰 간격 (초)")]
    public float spawnInterval = 2f;

    public float maxRandomAngle = 360f; // 360도로 회전 가능

    private float nextSpawnTime;
    private bool isSpawning = false; // 스폰 활성화/비활성화 상태를 관리하는 변수

    void Start()
    {
        // 시작 시에는 스폰을 하지 않도록 초기화
        isSpawning = false;
        nextSpawnTime = float.MaxValue; // 처음에는 스폰이 일어나지 않도록 큰 값으로 설정
    }

    void Update()
    {
        if (isSpawning && Time.time >= nextSpawnTime)
        {
            SpawnObject();
            nextSpawnTime = Time.time + spawnInterval; // 다음 스폰 시간 업데이트
        }
    }

    void SpawnObject()
    {
        if (objectToSpawn != null)
        {
            // 랜덤한 스폰 위치 계산
            float randomX = Random.Range(spawnCenter.x - spawnRangeX, spawnCenter.x + spawnRangeX);
            float randomY = Random.Range(spawnCenter.y - spawnRangeY, spawnCenter.y + spawnRangeY);
            Vector3 randomSpawnPosition = new Vector3(randomX, randomY, spawnCenter.z); // Z축은 중심 위치와 동일하게 유지 (2D 게임 기준)

            // 랜덤한 회전 각도 계산
            float randomRotationZ = Random.Range(0f, maxRandomAngle);
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomRotationZ); // 2D 게임에서는 Z축 회전만 고려

            // 오브젝트 스폰 (랜덤 회전 적용)
            Instantiate(objectToSpawn, randomSpawnPosition, randomRotation); // randomRotation 적용!
            // Debug.Log("Spawned Object Local Rotation: " + randomRotation.eulerAngles); // Quaternion의 오일러 각도 출력
        }
        else
        {
            Debug.LogError("스폰할 오브젝트가 지정되지 않았습니다!", gameObject);
        }
    }

    // 외부에서 스폰을 시작하는 함수
    public void StartSpawning()
    {
        isSpawning = true;
        nextSpawnTime = Time.time + spawnInterval; // 스폰 시작 시 첫 스폰 시간 설정
    }

    // 외부에서 스폰을 멈추는 함수
    public void StopSpawning()
    {
        isSpawning = false;
        nextSpawnTime = float.MaxValue; // 스폰이 멈추면 다음 스폰 시간을 아주 먼 미래로 설정
    }

    // (선택 사항) 에디터에서 스폰 범위를 시각적으로 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnCenter, new Vector3(spawnRangeX * 2, spawnRangeY * 2, 1f)); // Z축 크기는 임의로 1f 설정
    }
}