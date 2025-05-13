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

    //[Tooltip("���� �ð��� ǥ���� �ؽ�Ʈ UI")]
    //public TextMeshProUGUI progressText_TMP; // TextMeshPro ��� ��


    [Tooltip("���� �ð��� ǥ���� �����̴�")]
    public Slider slider;
    private CanvasGroup sliderCanvasGroup;
    public float fadeDuration = 1.0f;

    private bool isRunning = false; // ��ɵ��� ���۵Ǿ����� Ȯ���ϴ� �÷���
    private float startTime; // ���� �ð� ���

    void Start()
    {
        sliderCanvasGroup = slider.GetComponent<CanvasGroup>();
        if (sliderCanvasGroup != null)
        {
            sliderCanvasGroup.alpha = 0f;
            sliderCanvasGroup.interactable = false;
            sliderCanvasGroup.blocksRaycasts = false;
        }

        SetMaxHealth(activeDuration);
        conveyorBelt.gameObject.SetActive(false);
       
    }

    public void StartAllFeatures()
    {
        StartCoroutine(FadeInSlider());
        conveyorBelt.gameObject.SetActive(true);
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
        UpdateProgressUI();
    }
    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(float health)
    {
        slider.value = health;
    }
    void UpdateProgressUI()
    {
        if (slider != null)
        {
            float elapsed = Time.time - startTime;
            float remaining = Mathf.Clamp(activeDuration - elapsed, 0f, activeDuration);
            SetHealth(remaining);
        }
        else
        {
            Debug.LogWarning("�����̴� UI�� ������� �ʾҽ��ϴ�!");
        }
    }

    IEnumerator FadeInSlider()
    {
        if (sliderCanvasGroup == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            sliderCanvasGroup.alpha = alpha;
            yield return null;
        }

        sliderCanvasGroup.alpha = 1f;
        sliderCanvasGroup.interactable = true;
        sliderCanvasGroup.blocksRaycasts = true;
    }

}