using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Slider ���
using TMPro;         // TextMeshPro ��� (�ּ� ó����)

public class BossManager : MonoBehaviour
{
    [Tooltip("������Ʈ ���� �Ŵ���")]
    public SpawnManager spawnManager;

    [Tooltip("�����̾� ��Ʈ ����")]
    public ConveyorBeltPhysics conveyorBelt;

    [Tooltip("��� ������ (RunningMove)")]
    public RunningMove backgroundMover;

    public GameObject telSpot;
    public SpriteFader spriteFader; // SpriteFader ��ũ��Ʈ ������Ʈ ����

    [Tooltip("���� ���� �ð� (��)")]
    public float startDelay = 2f;
    [Tooltip("�۵� ���� �ð� (��)")]
    public float activeDuration = 6f;

    //[Tooltip("���� �ð��� ǥ���� �ؽ�Ʈ UI")]
    //public TextMeshProUGUI progressText_TMP; // TextMeshPro ��� ��

    [Tooltip("���� �ð��� ǥ���� �����̴�")]
    public Slider slider;
    private CanvasGroup sliderCanvasGroup;
    public float fadeDuration = 1.0f; // �����̴� ���̵� ��/�ƿ� ���� �ð�

    private bool isRunning = false; // ��ɵ��� ���۵Ǿ����� Ȯ���ϴ� �÷���
    private float startTime; // ���� �ð� ���

    void Start()
    {
        // �����̴��� CanvasGroup ������Ʈ ã�� (������ �߰�)
        sliderCanvasGroup = slider.GetComponent<CanvasGroup>();
        if (sliderCanvasGroup == null)
        {
            sliderCanvasGroup = slider.gameObject.AddComponent<CanvasGroup>();
        }

        // �ʱ� ����: �����̴� ����
        sliderCanvasGroup.alpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;

        SetMaxHealth(activeDuration);

        // �����̾� ��Ʈ �ʱ� ��Ȱ��ȭ
        if (conveyorBelt != null)
        {
            conveyorBelt.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("BossManager: ConveyorBelt�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void StartAllFeatures()
    {
        MainSoundManager.instance.ChangeBGM("1StageBoss");
        StartCoroutine(FadeInSlider()); // �����̴� ������ ��Ÿ���� ��

        if (conveyorBelt != null)
        {
            conveyorBelt.gameObject.SetActive(true); // �����̾� ��Ʈ Ȱ��ȭ
            conveyorBelt.StartConveying();
            Debug.Log("�����̾� ��Ʈ ����");
        }
        else
        {
            Debug.LogWarning("BossManager: �����̾� ��Ʈ�� ������� �ʾҽ��ϴ�.");
        }

        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
            Debug.Log("���� �Ŵ��� ����");
        }
        else
        {
            Debug.LogWarning("BossManager: ���� �Ŵ����� ������� �ʾҽ��ϴ�.");
        }

        if (spriteFader != null)
        {
            spriteFader.StartFadeIn(); // SpriteFader ����
            Debug.Log("��������Ʈ ���̴� ����");
        }
        else
        {
            Debug.LogWarning("BossManager: SpriteFader�� ������� �ʾҽ��ϴ�.");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StartScrolling();
            Debug.Log("��� ������ ����");
        }
        else
        {
            Debug.LogWarning("BossManager: ��� �������� ������� �ʾҽ��ϴ�.");
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
                DialogueManager.instance.StartDialogueByIdRange("1B-3s", "1B-3e");
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

    // ��� ����� ���ߴ� �Լ�
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

        // �����̴� ��Ȱ��ȭ �� ����
        StartCoroutine(FadeOutSlider());

        // spriteFader�� ���� GameObject �ı� ���� �Ͼ�� ���ߴٰ� ������� ȿ�� ����
        if (spriteFader != null)
        {
            // �ڷ�ƾ�� �Ϸ�� ������ ��ٷȴٰ� GameObject �ı�
            StartCoroutine(DestroySpriteFaderAfterFadeOut(spriteFader));
        }
        else
        {
            Debug.LogWarning("BossManager: ������ SpriteFader ��ü�� �����ϴ�.");
        }

        Debug.Log("��� ��� �ߴ�!");
    }
    private IEnumerator DestroySpriteFaderAfterFadeOut(SpriteFader fader)
    {
        if (fader == null) yield break;

        yield return fader.CoFadeOutWhiteWipe(); // SpriteFader�� ���ο� �ڷ�ƾ ȣ�� �� �Ϸ� ���

        // ���̵� �ƿ��� �Ϸ�� �� GameObject �ı�
        Destroy(fader.gameObject);
        Debug.Log("SpriteFader ��ü ���� �Ϸ�");
        spriteFader = null; // ���� ����
    }
    public void SetMaxHealth(float health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
    }

    public void SetHealth(float health)
    {
        if (slider != null)
        {
            slider.value = health;
        }
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
            // Debug.LogWarning("�����̴� UI�� ������� �ʾҽ��ϴ�!"); // �ʹ� ���� ��µ� �� ������ �ּ� ó��
        }
    }

    IEnumerator FadeInSlider()
    {
        if (sliderCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        sliderCanvasGroup.interactable = true;
        sliderCanvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            sliderCanvasGroup.alpha = alpha;
            yield return null;
        }

        sliderCanvasGroup.alpha = 1f;
    }

    // �����̴��� ������ ������� �ϴ� �ڷ�ƾ �߰�
    IEnumerator FadeOutSlider()
    {
        if (sliderCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        float currentAlpha = sliderCanvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(currentAlpha, 0f, elapsed / fadeDuration);
            sliderCanvasGroup.alpha = alpha;
            yield return null;
        }

        sliderCanvasGroup.alpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;
    }

    public void ReduceActiveDuration(float amount)
    {
        activeDuration = Mathf.Max(0f, activeDuration - amount); // 0�� �����δ� ���� �ʵ���
        SetMaxHealth(activeDuration); // �����̴� �ִ밪�� ������Ʈ
        Debug.Log($"activeDuration�� {amount}�� �����Ͽ� ���� {activeDuration}���Դϴ�.");
    }
}