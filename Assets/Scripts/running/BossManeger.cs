using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Tooltip("������Ʈ ���� �Ŵ���")]
    public SpawnManager spawnManager;

    [Tooltip("�����̾� ��Ʈ ����")]
    public ConveyorBeltPhysics conveyorBelt;

    [Tooltip("��� ������ (RunningMove)")]
    public RunningMove backgroundMover;

    public GameObject telSpot;

    [Tooltip("���� ���� �ð� (��)")]
    public float startDelay = 2f;
    [Tooltip("�۵� ���� �ð� (��)")]
    public float activeDuration = 6f;

    private bool isRunning = false; // ��ɵ��� ���۵Ǿ����� Ȯ���ϴ� �÷���
    private float startTime; // ���� �ð� ���

    void Start()
    {
        // ������ ���� �ð� �Ŀ� ��� ��� ����
       // Invoke("StartAllFeatures", startDelay);
    }

    public void StartAllFeatures()
    {
        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
            Debug.Log("���� �Ŵ��� ����");
        }
        else
        {
            Debug.LogWarning("���� �Ŵ����� ������� �ʾҽ��ϴ�.");
        }

        if (conveyorBelt != null)
        {
            conveyorBelt.StartConveying();
            Debug.Log("�����̾� ��Ʈ ����");
        }
        else
        {
            Debug.LogWarning("�����̾� ��Ʈ�� ������� �ʾҽ��ϴ�.");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StartScrolling();
            Debug.Log("��� ������ ����");
        }
        else
        {
            Debug.LogWarning("��� �������� ������� �ʾҽ��ϴ�.");
        }

        Debug.Log("��� ��� ����!");
        isRunning = true;
        startTime = Time.time; // ���� �ð� ���
    }

    void Update()
    {
        if (isRunning)
        {
            // ���� �� activeDuration �ð��� �������� StopAllFeatures ȣ��
            if (Time.time >= startTime + activeDuration)
            {
                StopAllFeatures();
                isRunning = false; // �� �̻� Update���� Ȯ������ �ʵ��� �÷��� ����
            }
        }
    }

    // (���� ����) �ʿ��ϴٸ� ��� ����� ���ߴ� �Լ�
    public void StopAllFeatures()
    {
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
            Debug.Log("���� �Ŵ��� �ߴ�");
        }

        if (conveyorBelt != null)
        {
            conveyorBelt.StopConveying();
            Destroy(conveyorBelt.gameObject); // �����̾� ��Ʈ ������Ʈ ��ü�� ����
            conveyorBelt = null; // ���� ����
            Debug.Log("�����̾� ��Ʈ �ߴ� �� ����");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StopScrolling();
            Debug.Log("��� ������ �ߴ�");
        }

        if (telSpot != null)
        {
            Destroy(telSpot);
            Debug.Log("�ڷ���Ʈ ���� ����");
            telSpot = null; // ���� ����
        }

        Debug.Log("��� ��� �ߴ�!");
    }
}