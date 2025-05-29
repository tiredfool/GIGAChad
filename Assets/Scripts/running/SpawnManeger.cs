using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Tooltip("������ ������Ʈ")]
    public GameObject objectToSpawn;

    [Tooltip("���� ������ �߽� ��ġ")]
    public Vector3 spawnCenter = Vector3.zero;

    [Tooltip("���� ������ X�� ���� ����")]
    public float spawnRangeX = 5f;

    [Tooltip("���� ������ Y�� ���� ����")]
    public float spawnRangeY = 3f;

    [Tooltip("���� ���� (��)")]
    public float spawnInterval = 2f;

    public float maxRandomAngle = 360f; // 360���� ȸ�� ����

    private float nextSpawnTime;
    private bool isSpawning = false; // ���� Ȱ��ȭ/��Ȱ��ȭ ���¸� �����ϴ� ����

    void Start()
    {
        // ���� �ÿ��� ������ ���� �ʵ��� �ʱ�ȭ
        isSpawning = false;
        nextSpawnTime = float.MaxValue; // ó������ ������ �Ͼ�� �ʵ��� ū ������ ����
    }

    void Update()
    {
        if (isSpawning && Time.time >= nextSpawnTime)
        {
            SpawnObject();
            nextSpawnTime = Time.time + spawnInterval; // ���� ���� �ð� ������Ʈ
        }
    }

    void SpawnObject()
    {
        if (objectToSpawn != null)
        {
            // ������ ���� ��ġ ���
            float randomX = Random.Range(spawnCenter.x - spawnRangeX, spawnCenter.x + spawnRangeX);
            float randomY = Random.Range(spawnCenter.y - spawnRangeY, spawnCenter.y + spawnRangeY);
            Vector3 randomSpawnPosition = new Vector3(randomX, randomY, spawnCenter.z); // Z���� �߽� ��ġ�� �����ϰ� ���� (2D ���� ����)

            // ������ ȸ�� ���� ���
            float randomRotationZ = Random.Range(0f, maxRandomAngle);
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomRotationZ); // 2D ���ӿ����� Z�� ȸ���� ���

            // ������Ʈ ���� (���� ȸ�� ����)
            Instantiate(objectToSpawn, randomSpawnPosition, randomRotation); // randomRotation ����!
            // Debug.Log("Spawned Object Local Rotation: " + randomRotation.eulerAngles); // Quaternion�� ���Ϸ� ���� ���
        }
        else
        {
            Debug.LogError("������ ������Ʈ�� �������� �ʾҽ��ϴ�!", gameObject);
        }
    }

    // �ܺο��� ������ �����ϴ� �Լ�
    public void StartSpawning()
    {
        isSpawning = true;
        nextSpawnTime = Time.time + spawnInterval; // ���� ���� �� ù ���� �ð� ����
    }

    // �ܺο��� ������ ���ߴ� �Լ�
    public void StopSpawning()
    {
        isSpawning = false;
        nextSpawnTime = float.MaxValue; // ������ ���߸� ���� ���� �ð��� ���� �� �̷��� ����
    }

    // (���� ����) �����Ϳ��� ���� ������ �ð������� ǥ��
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnCenter, new Vector3(spawnRangeX * 2, spawnRangeY * 2, 1f)); // Z�� ũ��� ���Ƿ� 1f ����
    }
}