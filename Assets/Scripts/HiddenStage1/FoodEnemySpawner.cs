using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs; // 여러 종류의 몬스터
    public float spawnRate = 1f;

    private Vector2[] spawnPoints = new Vector2[]
    {
        new Vector2(54.5f, 52f),
        new Vector2(47f, 45.3f),
        new Vector2(47f, 59.6f),
        new Vector2(39.2f, 52f)
    };

    void Start()
    {
        InvokeRepeating(nameof(SpawnMonster), 2f, spawnRate);
    }

    void SpawnMonster()
    {
        Vector2 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)]; // 4개 좌표 중 하나 선택
        GameObject monsterPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)]; // 몬스터 종류 랜덤 선택

        Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
    }
}