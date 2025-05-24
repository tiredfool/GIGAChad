using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 추가

public class BlackPanel : MonoBehaviour
{
    public GameObject FadePannel;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 0.5f;

    // Intro 스크립트가 DontDestroyOnLoad 상태일 경우, 이 코드를 사용하세요.
    // 이 오브젝트가 처음으로 활성화되거나, 씬 전환 후 다시 활성화될 때마다 호출됩니다.
    void OnEnable()
    {
        Debug.Log("<color=yellow>Intro OnEnable() 호출됨. 페이드 시작 시도.</color>");
        InitializeAndStartFade();
    }

    // 선택적으로, 특정 씬이 로드될 때만 페이드를 시작하고 싶다면 SceneManager.sceneLoaded를 사용합니다.
    // void Awake()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // void OnDestroy() // 오브젝트가 파괴될 때 이벤트 구독 해제
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     Debug.Log($"<color=yellow>Intro: 씬 '{scene.name}' 로드됨. 페이드 시작 시도.</color>");
    //     // 만약 특정 씬에서만 페이드하고 싶다면:
    //     // if (scene.name == "YourGameScene")
    //     // {
    //     //     InitializeAndStartFade();
    //     // }
    //     InitializeAndStartFade(); // 모든 씬 로드 시 페이드 시작
    // }

    private void InitializeAndStartFade()
    {
        // 이미 페이드 코루틴이 실행 중일 수도 있으니 중복 실행 방지
        StopAllCoroutines();

        if (FadePannel == null)
        {
            Debug.LogError("Intro 스크립트에 FadePannel이 할당되지 않았습니다!", this);
            enabled = false;
            return;
        }

        Image fadeImage = FadePannel.GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("FadePannel GameObject에 Image 컴포넌트가 없습니다!", FadePannel);
            enabled = false;
            return;
        }

        // 초기화: 패널 활성화 및 색상 설정
        FadePannel.SetActive(true);
        fadeImage.color = new Color(0,0,0, 1f);
        Debug.Log("FadePannel을 활성화하고 초기 색상을 흰색으로 설정했습니다. 코루틴 시작.");

        // 코루틴 시작
        StartCoroutine(FadeOutAndDeactivate());
    }

    IEnumerator FadeOutAndDeactivate()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        Image fadeImage = FadePannel.GetComponent<Image>();
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        fadeImage.color = endColor;
        FadePannel.SetActive(false);
        Debug.Log("페이드 아웃 완료 후 FadePannel 비활성화.");

        // (선택 사항) 이 스크립트 인스턴스가 더 이상 필요 없으면 비활성화
        // enabled = false;
    }
}