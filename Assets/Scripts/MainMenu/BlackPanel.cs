using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManager�� ����ϱ� ���� �߰�

public class BlackPanel : MonoBehaviour
{
    public GameObject FadePannel;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 0.5f;

    // Intro ��ũ��Ʈ�� DontDestroyOnLoad ������ ���, �� �ڵ带 ����ϼ���.
    // �� ������Ʈ�� ó������ Ȱ��ȭ�ǰų�, �� ��ȯ �� �ٽ� Ȱ��ȭ�� ������ ȣ��˴ϴ�.
    void OnEnable()
    {
        Debug.Log("<color=yellow>Intro OnEnable() ȣ���. ���̵� ���� �õ�.</color>");
        InitializeAndStartFade();
    }

    // ����������, Ư�� ���� �ε�� ���� ���̵带 �����ϰ� �ʹٸ� SceneManager.sceneLoaded�� ����մϴ�.
    // void Awake()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // void OnDestroy() // ������Ʈ�� �ı��� �� �̺�Ʈ ���� ����
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     Debug.Log($"<color=yellow>Intro: �� '{scene.name}' �ε��. ���̵� ���� �õ�.</color>");
    //     // ���� Ư�� �������� ���̵��ϰ� �ʹٸ�:
    //     // if (scene.name == "YourGameScene")
    //     // {
    //     //     InitializeAndStartFade();
    //     // }
    //     InitializeAndStartFade(); // ��� �� �ε� �� ���̵� ����
    // }

    private void InitializeAndStartFade()
    {
        // �̹� ���̵� �ڷ�ƾ�� ���� ���� ���� ������ �ߺ� ���� ����
        StopAllCoroutines();

        if (FadePannel == null)
        {
            Debug.LogError("Intro ��ũ��Ʈ�� FadePannel�� �Ҵ���� �ʾҽ��ϴ�!", this);
            enabled = false;
            return;
        }

        Image fadeImage = FadePannel.GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("FadePannel GameObject�� Image ������Ʈ�� �����ϴ�!", FadePannel);
            enabled = false;
            return;
        }

        // �ʱ�ȭ: �г� Ȱ��ȭ �� ���� ����
        FadePannel.SetActive(true);
        fadeImage.color = new Color(0,0,0, 1f);
        Debug.Log("FadePannel�� Ȱ��ȭ�ϰ� �ʱ� ������ ������� �����߽��ϴ�. �ڷ�ƾ ����.");

        // �ڷ�ƾ ����
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
        Debug.Log("���̵� �ƿ� �Ϸ� �� FadePannel ��Ȱ��ȭ.");

        // (���� ����) �� ��ũ��Ʈ �ν��Ͻ��� �� �̻� �ʿ� ������ ��Ȱ��ȭ
        // enabled = false;
    }
}