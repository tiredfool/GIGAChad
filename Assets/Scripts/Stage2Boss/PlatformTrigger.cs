using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab;      // ������ ����� �÷��� ������
    private bool hasStepped = false;       // �ߺ� ���� ����
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("�÷��̾ ������ ����");

        // ���ο� ���� ��ġ: Y�� ���纸�� 3 ��, X�� ����
        Vector3 currentPos = transform.position;
        float newX = Random.Range(269f, 280f);
        float newY = currentPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);

        // ���ο� ���� ����
        GameObject newPlatform = Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);
        newPlatform.GetComponent<PlatformTrigger>().platformPrefab = platformPrefab;

        // ���� ������ 3�� �� ����
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }
}
