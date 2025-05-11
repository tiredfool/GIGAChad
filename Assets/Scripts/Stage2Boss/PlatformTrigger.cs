using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab;       // ������ ����� �÷��� ������
    public GameObject blackWaves;           // ������ �ö�� BlackWaves ������Ʈ
    public GameObject mainCamera;           // ���� ī�޶�
    public GameObject bossCamera;           // ������ ī�޶�

    private bool hasStepped = false;
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("�÷��̾ ������ ����");

        // ���� ī�޶� ��ȯ
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // BlackWaves �ö���� ����
        if (blackWaves != null)
        {
            StartCoroutine(RaiseBlackWaves());
        }

        // ���� ���� ����
        Vector3 currentPos = transform.position;
        float newX = Random.Range(270f, 275f);
        float newY = currentPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);

        GameObject newPlatform = Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);
        newPlatform.GetComponent<PlatformTrigger>().platformPrefab = platformPrefab;
        newPlatform.GetComponent<PlatformTrigger>().blackWaves = blackWaves;
        newPlatform.GetComponent<PlatformTrigger>().mainCamera = mainCamera;
        newPlatform.GetComponent<PlatformTrigger>().bossCamera = bossCamera;

        // ���� ���� ���� ����
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator RaiseBlackWaves()
    {
        float duration = 7f;
        float elapsed = 0f;
        Vector3 startPos = blackWaves.transform.position;
        Vector3 targetPos = startPos + new Vector3(0f, 20f, 0f); // 20��ŭ ���

        while (elapsed < duration)
        {
            blackWaves.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        blackWaves.transform.position = targetPos;
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }
}