using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;

    private bool hasStepped = false;
    private float destroyDelay = 3f;

    // 카메라 자동 상승용 설정
    public float cameraRiseSpeed = 2f;
    private bool isCameraRising = false;

    void Update()
    {
        // 카메라가 상승할 때, BlackWaves도 함께 상승
        if (isCameraRising && bossCamera != null)
        {
            bossCamera.transform.position += new Vector3(0f, cameraRiseSpeed * Time.deltaTime, 0f);

            // BlackWaves도 함께 상승
            if (blackWaves != null)
            {
                blackWaves.transform.position += new Vector3(0f, cameraRiseSpeed * Time.deltaTime, 0f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("플레이어가 플랫폼에 닿음");

        // 점수 추가
        ScoreManager.instance.AddScore(100);

        // 카메라 전환
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);

        // 카메라 상승 시작
        CameraSmoothRise cameraScript = bossCamera.GetComponent<CameraSmoothRise>();
        if (cameraScript != null && !cameraScript.startRising)
        {
            cameraScript.StartRising();  // 상승 시작
        }

        // BlackWaves 이동 시작
        if (blackWaves != null)
        {
            StartCoroutine(RaiseBlackWaves());
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


    IEnumerator RaiseBlackWaves()
    {
        float duration = 7f;
        float elapsed = 0f;
        Vector3 startPos = blackWaves.transform.position;
        float targetY = startPos.y + 20f;

        while (elapsed < duration)
        {
            blackWaves.transform.position = Vector3.Lerp(startPos, new Vector3(startPos.x, targetY, startPos.z), elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        blackWaves.transform.position = new Vector3(startPos.x, targetY, startPos.z);
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }

}
