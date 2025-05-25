using UnityEngine;
using System.Collections; // Coroutine�� ����ϱ� ���� �ʿ�

public class FadeOutSprite : MonoBehaviour
{
    public float fadeDuration = 1.0f; // ���̵� �ƿ��� �Ϸ�Ǵ� �ð� (��)

    // �ܺο��� �� �Լ��� ȣ���Ͽ� ���̵� �ƿ��� �����մϴ�.
    public void StartFadeOut()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("FadeOutSprite: SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�.", this);
            return;
        }

        StartCoroutine(FadeOutCoroutine(spriteRenderer));
    }

    private IEnumerator FadeOutCoroutine(SpriteRenderer sr)
    {
        Color startColor = sr.color; // ���� ��������Ʈ�� ���� (���� �� ����)
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // ������ ������ ����

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration; // 0���� 1���� ����
            sr.color = Color.Lerp(startColor, endColor, normalizedTime); // ���� ����

            yield return null; // ���� �����ӱ��� ���
        }

        sr.color = endColor; // Ȥ�� �� ������ ���� ���������� ������ �����ϰ� ����

        // ���̵� �ƿ� �Ϸ� �� ���� ������Ʈ ��Ȱ��ȭ �Ǵ� �ı�
        gameObject.SetActive(false); // �Ǵ� Destroy(gameObject);
    }

    // ����: Ư�� ������ �ڵ����� ���̵� �ƿ� �����Ϸ��� �ּ��� �����ϼ���.
    // void Start()
    // {
    //     StartFadeOut();
    // }
}