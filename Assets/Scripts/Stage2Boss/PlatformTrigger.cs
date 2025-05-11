using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab; // Platform ������
    private bool hasTriggered = false; // �ߺ� ������ �÷���

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return; // �̹� ����ƴٸ� ����
        if (!other.CompareTag("Player")) return;

        Debug.Log("�÷��̾ ���� Ʈ���ſ� ����");

        hasTriggered = true; // ù ���� ���� �ٽô� ���� �ȵ�

        // ���� ���� ���� �õ�
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        Debug.Log("���� ����: " + platforms.Length);

        if (platforms.Length == 0) return;

        GameObject chosen = platforms[Random.Range(0, platforms.Length)];
        Vector3 spawnPos = chosen.transform.position + new Vector3(0, 3, 0);

        // �ߺ� ��ġ ����: ���� Y�� �̹� �ִ��� Ȯ��
        foreach (GameObject p in platforms)
        {
            if (Mathf.Approximately(p.transform.position.y, spawnPos.y))
            {
                Debug.Log("���� ���̿� �̹� ������ �־� ���� ����");
                return;
            }
        }

        Debug.Log("�� ���� ���� ��ġ: " + spawnPos);

        Instantiate(platformPrefab, spawnPos, Quaternion.identity);

        // Tilemap �����̱� ����
        StartCoroutine(FlickerAndDestroy());
    }

    IEnumerator FlickerAndDestroy()
    {
        TilemapRenderer renderer = GetComponentInParent<TilemapRenderer>();
        GameObject toDestroy = transform.parent.gameObject;

        if (renderer == null)
        {
            Debug.LogWarning("TilemapRenderer�� �θ� ������Ʈ�� �����ϴ�.");
            yield break;
        }

        float duration = 3f;
        float interval = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            renderer.enabled = !renderer.enabled;
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        Destroy(toDestroy); // �θ� ������Ʈ ����
    }

}
