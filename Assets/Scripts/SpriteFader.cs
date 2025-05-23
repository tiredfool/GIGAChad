using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float fadeInDuration = 1.0f; // 나타나는 시간
    public float fadeOutDuration = 1.0f; // 사라지는 시간 (하얗게 변했다가 사라짐)

    private Material originalMaterial; // 원래 머티리얼을 저장
    private Material fadeMaterial; // 쉐이더를 적용할 새로운 머티리얼

    private readonly string _WIPE_AMOUNT_PROP = "_WipeAmount"; // 쉐이더 프로퍼티 이름 (명확화를 위해 이름 변경)
    private readonly string _FADE_AMOUNT_PROP = "_FadeAmount"; // 쉐이더 프로퍼티 이름 (명확화를 위해 이름 변경)
    private readonly string _COLOR_PROP = "_Color"; // 쉐이더의 Tint Color 프로퍼티 이름

    private void Awake() // Start보다 먼저 Awake에서 초기화
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteFader: SpriteRenderer 컴포넌트를 찾을 수 없습니다. 이 스크립트는 비활성화됩니다.", this);
                enabled = false;
                return;
            }
        }

        // SpriteRenderer의 현재 머티리얼을 저장 (게임 시작 시 할당된 기본 머티리얼)
        // 이 머티리얼은 오브젝트가 파괴될 때 원래대로 되돌리는 데 사용됩니다.
        originalMaterial = spriteRenderer.material;

        // 쉐이더를 찾아서 새로운 머티리얼 인스턴스 생성
        // Resources 폴더에 있다면 Resources.Load<Shader>("Path/To/WhiteWipeOutShader");
        // 아니면 에디터에서 public Shader 변수로 할당하는 것도 좋은 방법입니다.
        Shader customShader = Shader.Find("Custom/WhiteWipeOutShader");
        if (customShader == null)
        {
            Debug.LogError("SpriteFader: 'Custom/WhiteWipeOutShader' 쉐이더를 찾을 수 없습니다. 쉐이더 이름이 맞는지 확인하세요.", this);
            enabled = false;
            return;
        }
        fadeMaterial = new Material(customShader);
    }

    // 오브젝트가 활성화될 때마다 호출됩니다.
    private void OnEnable()
    {
        if (spriteRenderer != null && fadeMaterial != null)
        {
            spriteRenderer.material = fadeMaterial; // 커스텀 머티리얼 적용

            // 페이드 인을 위해 쉐이더 프로퍼티 초기화
            // _WipeAmount: 0 (모두 보임)
            // _FadeAmount: 0 (원래 색상)
            // _Color (알파): 0 (완전히 투명)
            spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 0f);
            spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 0f);
            spriteRenderer.material.SetColor(_COLOR_PROP, new Color(1, 1, 1, 0)); // 시작 알파를 0으로 설정
        }
    }

    // 오브젝트가 비활성화될 때 또는 스크립트가 비활성화될 때 호출
    private void OnDisable()
    {
        // 원래 머티리얼로 되돌려놓아야 다른 스크립트나 기본 동작에 영향을 주지 않습니다.
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    // 스프라이트가 서서히 나타나게 함 (기존 기능 유지)
    public void StartFadeIn()
    {
        if (spriteRenderer == null || fadeMaterial == null) return;

        StopAllCoroutines(); // 혹시 다른 코루틴이 실행 중이면 중지 (예: FadeOut)
        StartCoroutine(CoFadeIn());
    }

    IEnumerator CoFadeIn()
    {
        float elapsed = 0f;

        // 페이드 인 시작 전에 쉐이더 프로퍼티를 초기화
        spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 0f); // 페이드 인 시에는 와이프 효과 없음 (모두 보임)
        spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 0f); // 페이드 인 시에는 흰색 효과 없음 (원래 색상)

        // _Color 프로퍼티의 알파값만 0에서 1로 조절합니다.
        Color startColor = new Color(1, 1, 1, 0); // 시작 알파 0
        Color targetColor = new Color(1, 1, 1, 1); // 목표 알파 1 (쉐이더의 _Color 프로퍼티에 영향을 줌)

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);
            Color currentColor = Color.Lerp(startColor, targetColor, progress);
            spriteRenderer.material.SetColor(_COLOR_PROP, currentColor); // 쉐이더의 _Color 프로퍼티 (알파) 조절
            yield return null;
        }
        spriteRenderer.material.SetColor(_COLOR_PROP, targetColor); // 완전히 보이게 고정 (알파 1)
        Debug.Log("SpriteFader: FadeIn 완료.");
    }

    // 스프라이트가 하얗게 변했다가 위에서부터 사라지게 함 (새로운 기능)
    public IEnumerator CoFadeOutWhiteWipe()
    {
        if (spriteRenderer == null || fadeMaterial == null) yield break;

        StopAllCoroutines(); // 혹시 다른 코루틴이 실행 중이면 중지 (예: FadeIn)

        spriteRenderer.material = fadeMaterial; // 페이드 머티리얼 적용이 확실히 되도록

        float elapsed = 0f;
        float fadeToWhiteDuration = fadeOutDuration * 0.4f; // 전체 사라지는 시간의 40%는 하얗게 변하는 시간
        float wipeOutDuration = fadeOutDuration * 0.6f;     // 나머지 60%는 위에서부터 사라지는 시간

        // 1. 하얗게 변하는 페이드 (현재 색상에서 흰색으로)
        // 현재 쉐이더의 _Color 프로퍼티에서 알파값을 1로 고정하고, RGB만 조절.
        Color startCurrentColor = spriteRenderer.material.GetColor(_COLOR_PROP); // 현재 색상 (알파 1인 상태)

        while (elapsed < fadeToWhiteDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeToWhiteDuration);
            spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, progress); // _FadeAmount 0 -> 1 (원래 색 -> 흰색)
            yield return null;
        }
        spriteRenderer.material.SetFloat(_FADE_AMOUNT_PROP, 1f); // 완전히 하얗게 고정
        Debug.Log("SpriteFader: 흰색으로 페이드 완료.");


        // 2. 위에서부터 아래로 사라지는 와이프 아웃
        elapsed = 0f; // 시간 초기화

        // 와이프 아웃 시작 시 _Color의 알파는 여전히 1 (불투명)으로 유지
        // 쉐이더의 _WipeAmount가 알파를 제어하므로 _Color의 알파는 건드릴 필요 없음.

        while (elapsed < wipeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / wipeOutDuration);
            // _WipeAmount 0 -> 1 (0: 모두 보임, 1: 모두 사라짐)
            // 쉐이더에서 i.uv.y < _WipeAmount 이면 사라지므로, 
            // progress가 0일 때는 (y가 0보다 작은 부분만 사라지므로) 거의 모두 보이고,
            // progress가 1일 때는 (y가 1보다 작은 모든 부분이 사라지므로) 모두 사라집니다.
            spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, progress);
            yield return null;
        }
        spriteRenderer.material.SetFloat(_WIPE_AMOUNT_PROP, 1f); // 완전히 사라짐
        Debug.Log("SpriteFader: 와이프 아웃 완료.");

        // 완전히 사라진 후 _Color의 알파값도 0으로 명시적으로 설정 (선택 사항)
        // Destroy(fader.gameObject)가 호출되므로 필요 없을 수도 있습니다.
        Color finalTransparentColor = new Color(1, 1, 1, 0);
        spriteRenderer.material.SetColor(_COLOR_PROP, finalTransparentColor);

        // 이 스크립트가 붙은 GameObject를 파괴하는 것은 BossManager에서 담당합니다.
    }
}