using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchToTrigger : MonoBehaviour
{
    public enum SwitchType { Timed, Permanent }
    public SwitchType switchType = SwitchType.Timed; // ����ġ Ÿ�� ����

    public Sprite greenLightSprite;
    private Sprite originalSprite;

    private SpriteRenderer switchRenderer;
    private TilemapRenderer[] platformRenderers;
    private TilemapCollider2D[] platformColliders;

    public float activeTime = 10f; // Ÿ�̸� ��� Ȱ��ȭ �ð� (Timed Ÿ�Ը� ���)

    private bool isActivated = false;

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite;

        // Trick �±׸� ���� ��� ���� ã�� ��Ȱ��ȭ
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
                ActivatePlatforms(true); // �� ���� �۵�, ��� ����
            }
        }
    }

    IEnumerator ActivateTemporarily()
    {
        ActivatePlatforms(true); // ���� �ѱ�

        float blinkStartTime = activeTime - 3f;
        yield return new WaitForSeconds(blinkStartTime);

        // �����̱� ����
        float blinkDuration = 3f;
        float blinkInterval = 0.3f;
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            TogglePlatformRenderers();
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // ���� ���ֱ�
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
