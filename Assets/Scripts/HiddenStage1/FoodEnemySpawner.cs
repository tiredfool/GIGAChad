using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs; // ���� ������ ����
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
        Vector2 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)]; // 4�� ��ǥ �� �ϳ� ����
        GameObject monsterPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)]; // ���� ���� ���� ����

        Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
    }
}