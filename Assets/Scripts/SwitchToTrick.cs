using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchToTrigger : MonoBehaviour
{
    public enum SwitchType { Timed, Permanent }
    public SwitchType switchType = SwitchType.Timed; // 스위치 타입 선택

    public Sprite greenLightSprite;
    private Sprite originalSprite;

    private SpriteRenderer switchRenderer;
    private TilemapRenderer[] platformRenderers;
    private TilemapCollider2D[] platformColliders;

    public float activeTime = 10f; // 타이머 기반 활성화 시간 (Timed 타입만 사용)

    private bool isActivated = false;

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite;

        // Trick 태그를 가진 모든 발판 찾아 비활성화
        GameObject[] trickPlatforms = GameObject.FindGameObjectsWithTag("Trick");
        platformRenderers = new TilemapRenderer[trickPlatforms.Length];
        platformColliders = new TilemapCollider2D[trickPlatforms.Length];

        for (int i = 0; i < trickPlatforms.Length; i++)
        {
            platformRenderers[i] = trickPlatforms[i].GetComponent<TilemapRenderer>();
            platformColliders[i] = trickPlatforms[i].GetComponent<TilemapCollider2D>();

            if (platformRenderers[i] != null)
                platformRenderers[i].enabled = false;
            if (platformColliders[i] != null)
                platformColliders[i].enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (switchType == SwitchType.Timed)
        {
            if (!isActivated)
            {
                isActivated = true;
                switchRenderer.sprite = greenLightSprite;
                StartCoroutine(ActivateTemporarily());
            }
        }
        else if (switchType == SwitchType.Permanent)
        {
            if (!isActivated)
            {
                isActivated = true;
                switchRenderer.sprite = greenLightSprite;
                ActivatePlatforms(true); // 한 번만 작동, 계속 켜짐
            }
        }
    }

    IEnumerator ActivateTemporarily()
    {
        ActivatePlatforms(true); // 발판 켜기

        float blinkStartTime = activeTime - 3f;
        yield return new WaitForSeconds(blinkStartTime);

        // 깜빡이기 시작
        float blinkDuration = 3f;
        float blinkInterval = 0.3f;
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            TogglePlatformRenderers();
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 발판 꺼주기
        ActivatePlatforms(false);
        switchRenderer.sprite = originalSprite;
        isActivated = false;
    }

    void ActivatePlatforms(bool state)
    {
        for (int i = 0; i < platformRenderers.Length; i++)
        {
            if (platformRenderers[i] != null)
                platformRenderers[i].enabled = state;
            if (platformColliders[i] != null)
                platformColliders[i].enabled = state;
        }
    }

    void TogglePlatformRenderers()
    {
        for (int i = 0; i < platformRenderers.Length; i++)
        {
            if (platformRenderers[i] != null)
                platformRenderers[i].enabled = !platformRenderers[i].enabled;
        }
    }
}
