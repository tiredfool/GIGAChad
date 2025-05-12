using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text UI�� ����ϴ� ���
using TMPro;         // TextMeshPro�� ����ϴ� ���

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

    [Tooltip("���� �ð��� ǥ���� �ؽ�Ʈ UI")]
    public TextMeshProUGUI progressText_TMP; // TextMeshPro ��� ��
    //public Text progressText_Legacy;       // �⺻ Text UI ��� ��

    private bool isRunning = false; // ��ɵ��� ���۵Ǿ����� Ȯ���ϴ� �÷���
    private float startTime; // ���� �ð� ���

    void Start()
    {
        // ������ ���� �ð� �Ŀ� ��� ��� ����
        Invoke("StartAllFeatures", startDelay);
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

        // UI ������Ʈ ����
        UpdateProgressUI();
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
            else
            {
                // UI ������Ʈ
                UpdateProgressUI();
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

        // UI ������Ʈ (�ߴ� �޽��� ǥ��)
        UpdateProgressUI(true);
    }

    void UpdateProgressUI(bool isStopped = false)
    {
        if (progressText_TMP != null)
        {
            if (isStopped)
            {
                progressText_TMP.text = "PASS!";
            }
            else
            {
                float elapsedTime = Time.time - startTime;
                float progressPercentage = elapsedTime / activeDuration;
                progressPercentage = Mathf.Clamp01(progressPercentage); // 0�� 1 ���̷� ����

                string displayText = $"�ð�: {elapsedTime:F1} / {activeDuration:F1} \n���൵ : ({progressPercentage:P0})";
                progressText_TMP.text = displayText;
            }
        }
        // else if (progressText_Legacy != null)
        // {
        //     // ������ ������ ����Ͽ� progressText_Legacy�� ������Ʈ�մϴ�.
        // }
        else
        {
            Debug.LogWarning("���� �ð� �ؽ�Ʈ UI�� ������� �ʾҽ��ϴ�!");
        }
    }
}