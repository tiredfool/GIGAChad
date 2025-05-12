using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public static int steppedCount = 0;  // 밟은 횟수 전역 카운터

    public GameObject platformPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;

    private bool hasStepped = false;
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("플레이어가 플랫폼에 닿음");

        // 게임 UI 시작
        ScoreManager.instance.StartGameUI();

        // 점수 추가
        ScoreManager.instance.AddScore(100);

        // 밟은 횟수 증가
        steppedCount++;

        // 10개 단위로 상승 속도 증가
        if (steppedCount % 10 == 0)
        {
            Debug.Log($"플랫폼 {steppedCount}개 밟음 → 상승 속도 증가");
        }

        // 카메라 전환
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // 카메라 상승 시작
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();
        }

        // 새 플랫폼 생성
        Vector3 currentPos = transform.position;
        float newX = Random.Range(270f, 275f);
        float newY = currentPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);

        GameObject newPlatform = Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);
        var newTrigger = newPlatform.GetComponent<PlatformTrigger>();
        newTrigger.platformPrefab = platformPrefab;
        newTrigger.blackWaves = blackWaves;
        newTrigger.mainCamera = mainCamera;
        newTrigger.bossCamera = bossCamera;

        // 자신 삭제
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }
}
