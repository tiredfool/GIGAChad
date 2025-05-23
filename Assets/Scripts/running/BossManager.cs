using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Slider 사용
using TMPro;         // TextMeshPro 사용 (주석 처리됨)

public class BossManager : MonoBehaviour
{
    [Tooltip("오브젝트 스폰 매니저")]
    public SpawnManager spawnManager;

    [Tooltip("컨베이어 벨트 물리")]
    public ConveyorBeltPhysics conveyorBelt;

    [Tooltip("배경 움직임 (RunningMove)")]
    public RunningMove backgroundMover;

    public GameObject telSpot;
    public SpriteFader spriteFader; // SpriteFader 스크립트 컴포넌트 참조

    [Tooltip("시작 지연 시간 (초)")]
    public float startDelay = 2f;
    [Tooltip("작동 지속 시간 (초)")]
    public float activeDuration = 6f;

    //[Tooltip("진행 시간을 표시할 텍스트 UI")]
    //public TextMeshProUGUI progressText_TMP; // TextMeshPro 사용 시

    [Tooltip("진행 시간을 표시할 슬라이더")]
    public Slider slider;
    private CanvasGroup sliderCanvasGroup;
    public float fadeDuration = 1.0f; // 슬라이더 페이드 인/아웃 지속 시간

    private bool isRunning = false; // 기능들이 시작되었는지 확인하는 플래그
    private float startTime; // 시작 시간 기록

    void Start()
    {
        // 슬라이더의 CanvasGroup 컴포넌트 찾기 (없으면 추가)
        sliderCanvasGroup = slider.GetComponent<CanvasGroup>();
        if (sliderCanvasGroup == null)
        {
            sliderCanvasGroup = slider.gameObject.AddComponent<CanvasGroup>();
        }

        // 초기 상태: 슬라이더 숨김
        sliderCanvasGroup.alpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;

        SetMaxHealth(activeDuration);

        // 컨베이어 벨트 초기 비활성화
        if (conveyorBelt != null)
        {
            conveyorBelt.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("BossManager: ConveyorBelt가 할당되지 않았습니다.");
        }
    }

    public void StartAllFeatures()
    {
        MainSoundManager.instance.ChangeBGM("1StageBoss");
        StartCoroutine(FadeInSlider()); // 슬라이더 서서히 나타나게 함

        if (conveyorBelt != null)
        {
            conveyorBelt.gameObject.SetActive(true); // 컨베이어 벨트 활성화
            conveyorBelt.StartConveying();
            Debug.Log("컨베이어 벨트 시작");
        }
        else
        {
            Debug.LogWarning("BossManager: 컨베이어 벨트가 연결되지 않았습니다.");
        }

        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
            Debug.Log("스폰 매니저 시작");
        }
        else
        {
            Debug.LogWarning("BossManager: 스폰 매니저가 연결되지 않았습니다.");
        }

        if (spriteFader != null)
        {
            spriteFader.StartFadeIn(); // SpriteFader 시작
            Debug.Log("스프라이트 페이더 시작");
        }
        else
        {
            Debug.LogWarning("BossManager: SpriteFader가 연결되지 않았습니다.");
        }

        if (backgroundMover != null)
        {
            backgroundMover.StartScrolling();
            Debug.Log("배경 움직임 시작");
        }
        else
        {
            Debug.LogWarning("BossManager: 배경 움직임이 연결되지 않았습니다.");
        }

        Debug.Log("모든 기능 시작!");
        isRunning = true;
        startTime = Time.time; // 시작 시간 기록

        // UI 업데이트 시작
        UpdateProgressUI();
    }

    void Update()
    {
        if (isRunning)
        {
            // 시작 후 activeDuration 시간이 지났으면 StopAllFeatures 호출
            if (Time.time >= startTime + activeDuration)
            {
                DialogueManager.instance.StartDialogueByIdRange("1B-3s", "1B-3e");
                StopAllFeatures();
                isRunning = false; // 더 이상 Update에서 확인하지 않도록 플래그 변경
            }
            else
            {
                // UI 업데이트
                UpdateProgressUI();
            }
        }
    }

    // 모든 기능을 멈추는 함수
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

        // 슬라이더 비활성화 및 숨김
        StartCoroutine(FadeOutSlider());

        // spriteFader를 가진 GameObject 파괴 전에 하얗게 변했다가 사라지는 효과 시작
        if (spriteFader != null)
        {
            // 코루틴이 완료될 때까지 기다렸다가 GameObject 파괴
            StartCoroutine(DestroySpriteFaderAfterFadeOut(spriteFader));
        }
        else
        {
            Debug.LogWarning("BossManager: 제거할 SpriteFader 객체가 없습니다.");
        }

        Debug.Log("모든 기능 중단!");
    }
    private IEnumerator DestroySpriteFaderAfterFadeOut(SpriteFader fader)
    {
        if (fader == null) yield break;

        yield return fader.CoFadeOutWhiteWipe(); // SpriteFader의 새로운 코루틴 호출 및 완료 대기

        // 페이드 아웃이 완료된 후 GameObject 파괴
        Destroy(fader.gameObject);
        Debug.Log("SpriteFader 객체 제거 완료");
        spriteFader = null; // 참조 해제
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
            // Debug.LogWarning("슬라이더 UI가 연결되지 않았습니다!"); // 너무 자주 출력될 수 있으니 주석 처리
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

    // 슬라이더를 서서히 사라지게 하는 코루틴 추가
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
        activeDuration = Mathf.Max(0f, activeDuration - amount); // 0초 밑으로는 줄지 않도록
        SetMaxHealth(activeDuration); // 슬라이더 최대값도 업데이트
        Debug.Log($"activeDuration이 {amount}초 감소하여 현재 {activeDuration}초입니다.");
    }
}