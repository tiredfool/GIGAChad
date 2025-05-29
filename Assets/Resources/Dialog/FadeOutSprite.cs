using UnityEngine;
using System.Collections; // Coroutine을 사용하기 위해 필요

public class FadeOutSprite : MonoBehaviour
{
    public float fadeDuration = 1.0f; // 페이드 아웃이 완료되는 시간 (초)

    // 외부에서 이 함수를 호출하여 페이드 아웃을 시작합니다.
    public void StartFadeOut()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("FadeOutSprite: SpriteRenderer 컴포넌트를 찾을 수 없습니다.", this);
            return;
        }

        StartCoroutine(FadeOutCoroutine(spriteRenderer));
    }

    private IEnumerator FadeOutCoroutine(SpriteRenderer sr)
    {
        Color startColor = sr.color; // 현재 스프라이트의 색상 (알파 값 포함)
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 완전히 투명한 색상

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration; // 0에서 1까지 증가
            sr.color = Color.Lerp(startColor, endColor, normalizedTime); // 색상 보간

            yield return null; // 다음 프레임까지 대기
        }

        sr.color = endColor; // 혹시 모를 오차를 위해 최종적으로 완전히 투명하게 설정

        // 페이드 아웃 완료 후 게임 오브젝트 비활성화 또는 파괴
        gameObject.SetActive(false); // 또는 Destroy(gameObject);
    }

    // 예시: 특정 시점에 자동으로 페이드 아웃 시작하려면 주석을 해제하세요.
    // void Start()
    // {
    //     StartFadeOut();
    // }
}