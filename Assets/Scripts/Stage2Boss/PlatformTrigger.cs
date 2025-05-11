using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab;       // 생성에 사용할 플랫폼 프리팹
    public GameObject blackWaves;           // 서서히 올라올 BlackWaves 오브젝트
    public GameObject mainCamera;           // 기존 카메라
    public GameObject bossCamera;           // 보스용 카메라

    private bool hasStepped = false;
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("플레이어가 발판을 밟음");

        // 보스 카메라 전환
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // BlackWaves 올라오기 시작
        if (blackWaves != null)
        {
            StartCoroutine(RaiseBlackWaves());
        }

        // 다음 발판 생성
        Vector3 currentPos = transform.position;
        float newX = Random.Range(270f, 275f);
        float newY = currentPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);

        GameObject newPlatform = Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);
        newPlatform.GetComponent<PlatformTrigger>().platformPrefab = platformPrefab;
        newPlatform.GetComponent<PlatformTrigger>().blackWaves = blackWaves;
        newPlatform.GetComponent<PlatformTrigger>().mainCamera = mainCamera;
        newPlatform.GetComponent<PlatformTrigger>().bossCamera = bossCamera;

        // 현재 발판 제거 예약
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator RaiseBlackWaves()
    {
        float duration = 7f;
        float elapsed = 0f;
        Vector3 startPos = blackWaves.transform.position;
        Vector3 targetPos = startPos + new Vector3(0f, 20f, 0f); // 20만큼 상승

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