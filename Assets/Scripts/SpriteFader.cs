using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float fadeInDuration = 1.0f; // ��Ÿ���� �ð�
    public float fadeOutDuration = 1.0f; // ������� �ð� (�Ͼ�� ���ߴٰ� �����)

    private Material originalMaterial; // ���� ��Ƽ������ ����
    private Material fadeMaterial; // ���̴��� ������ ���ο� ��Ƽ����

    private readonly string _WIPE_AMOUNT_PROP = "_WipeAmount"; // ���̴� ������Ƽ �̸� (��Ȯȭ�� ���� �̸� ����)
    private readonly string _FADE_AMOUNT_PROP = "_FadeAmount"; // ���̴� ������Ƽ �̸� (��Ȯȭ�� ���� �̸� ����)
    private readonly string _COLOR_PROP = "_Color"; // ���̴��� Tint Color ������Ƽ �̸�

    private void Awake() // Start���� ���� Awake���� �ʱ�ȭ
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteFader: SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�. �� ��ũ��Ʈ�� ��Ȱ��ȭ�˴ϴ�.", this);
                enabled = false;
                return;
            }
        }

        // SpriteRenderer�� ���� ��Ƽ������ ���� (���� ���� �� �Ҵ�� �⺻ ��Ƽ����)
        // �� ��Ƽ������ ������Ʈ�� �ı��� �� ������� �ǵ����� �� ���˴ϴ�.
        originalMaterial = spriteRenderer.material;

        // ���̴��� ã�Ƽ� ���ο� ��Ƽ���� �ν��Ͻ� ����
        // Resources ������ �ִٸ� Resources.Load<Shader>("Path/To/WhiteWipeOutShader");
        // �ƴϸ� �����Ϳ��� public Shader ������ �Ҵ��ϴ� �͵� ���� ����Դϴ�.
        Shader customShader = Shader.Find("Custom/WhiteWipeOutShader");
        if (customShader == null)
        {
            Debug.LogError("SpriteFader: 'Custom/WhiteWipeOutShader' ���̴��� ã�� �� �����ϴ�. ���̴� �̸��� �´��� Ȯ���ϼ���.", this);
            enabled = false;
            return;
        }
        fadeMaterial = new Material(customShader);
    }

    // ������Ʈ�� Ȱ��ȭ�� ������ ȣ��˴ϴ�.
    private void OnEnable()
    {
        if (spriteRenderer != null && fadeMaterial != null)
        {
            spriteRenderer.material = fadeMaterial; // Ŀ���� ��Ƽ���� ����

            // ���̵� ���� ���� ���̴� ������Ƽ �ʱ�ȭ
            // _WipeAmount: 0 (��� ����)
            // _FadeAmount: 0 (���� ����)
            // _Color (����): 0 (������ ����)
            spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 0f);
            spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 0f);
            spriteRenderer.material.SetColor(_COLOR_PROP, new Color(1, 1, 1, 0)); // ���� ���ĸ� 0���� ����
        }
    }

    // ������Ʈ�� ��Ȱ��ȭ�� �� �Ǵ� ��ũ��Ʈ�� ��Ȱ��ȭ�� �� ȣ��
    private void OnDisable()
    {
        // ���� ��Ƽ����� �ǵ������ƾ� �ٸ� ��ũ��Ʈ�� �⺻ ���ۿ� ������ ���� �ʽ��ϴ�.
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    // ��������Ʈ�� ������ ��Ÿ���� �� (���� ��� ����)
    public void StartFadeIn()
    {
        if (spriteRenderer == null || fadeMaterial == null) return;

        StopAllCoroutines(); // Ȥ�� �ٸ� �ڷ�ƾ�� ���� ���̸� ���� (��: FadeOut)
        StartCoroutine(CoFadeIn());
    }

    IEnumerator CoFadeIn()
    {
        float elapsed = 0f;

        // ���̵� �� ���� ���� ���̴� ������Ƽ�� �ʱ�ȭ
        spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 0f); // ���̵� �� �ÿ��� ������ ȿ�� ���� (��� ����)
        spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 0f); // ���̵� �� �ÿ��� ��� ȿ�� ���� (���� ����)

        // _Color ������Ƽ�� ���İ��� 0���� 1�� �����մϴ�.
        Color startColor = new Color(1, 1, 1, 0); // ���� ���� 0
        Color targetColor = new Color(1, 1, 1, 1); // ��ǥ ���� 1 (���̴��� _Color ������Ƽ�� ������ ��)

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);
            Color currentColor = Color.Lerp(startColor, targetColor, progress);
            spriteRenderer.material.SetColor(_COLOR_PROP, currentColor); // ���̴��� _Color ������Ƽ (����) ����
            yield return null;
        }
        spriteRenderer.material.SetColor(_COLOR_PROP, targetColor); // ������ ���̰� ���� (���� 1)
        Debug.Log("SpriteFader: FadeIn �Ϸ�.");
    }

    // ��������Ʈ�� �Ͼ�� ���ߴٰ� ���������� ������� �� (���ο� ���)
    public IEnumerator CoFadeOutWhiteWipe()
    {
        if (spriteRenderer == null || fadeMaterial == null) yield break;

        StopAllCoroutines(); // Ȥ�� �ٸ� �ڷ�ƾ�� ���� ���̸� ���� (��: FadeIn)

        spriteRenderer.material = fadeMaterial; // ���̵� ��Ƽ���� ������ Ȯ���� �ǵ���

        float elapsed = 0f;
        float fadeToWhiteDuration = fadeOutDuration * 0.4f; // ��ü ������� �ð��� 40%�� �Ͼ�� ���ϴ� �ð�
        float wipeOutDuration = fadeOutDuration * 0.6f;     // ������ 60%�� ���������� ������� �ð�

        // 1. �Ͼ�� ���ϴ� ���̵� (���� ���󿡼� �������)
        // ���� ���̴��� _Color ������Ƽ���� ���İ��� 1�� �����ϰ�, RGB�� ����.
        Color startCurrentColor = spriteRenderer.material.GetColor(_COLOR_PROP); // ���� ���� (���� 1�� ����)

        while (elapsed < fadeToWhiteDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeToWhiteDuration);
            spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, progress); // _FadeAmount 0 -> 1 (���� �� -> ���)
            yield return null;
        }
        spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 1f); // ������ �Ͼ�� ����
        Debug.Log("SpriteFader: ������� ���̵� �Ϸ�.");


        // 2. ���������� �Ʒ��� ������� ������ �ƿ�
        elapsed = 0f; // �ð� �ʱ�ȭ

        // ������ �ƿ� ���� �� _Color�� ���Ĵ� ������ 1 (������)���� ����
        // ���̴��� _WipeAmount�� ���ĸ� �����ϹǷ� _Color�� ���Ĵ� �ǵ帱 �ʿ� ����.

        while (elapsed < wipeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / wipeOutDuration);
            // _WipeAmount 0 -> 1 (0: ��� ����, 1: ��� �����)
            // ���̴����� i.uv.y < _WipeAmount �̸� ������Ƿ�, 
            // progress�� 0�� ���� (y�� 0���� ���� �κи� ������Ƿ�) ���� ��� ���̰�,
            // progress�� 1�� ���� (y�� 1���� ���� ��� �κ��� ������Ƿ�) ��� ������ϴ�.
            spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, progress);
            yield return null;
        }
        spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 1f); // ������ �����
        Debug.Log("SpriteFader: ������ �ƿ� �Ϸ�.");

        // ������ ����� �� _Color�� ���İ��� 0���� ��������� ���� (���� ����)
        // Destroy(fader.gameObject)�� ȣ��ǹǷ� �ʿ� ���� ���� �ֽ��ϴ�.
        Color finalTransparentColor = new Color(1, 1, 1, 0);
        spriteRenderer.material.SetColor(_COLOR_PROP, finalTransparentColor);

        // �� ��ũ��Ʈ�� ���� GameObject�� �ı��ϴ� ���� BossManager���� ����մϴ�.
    }
}