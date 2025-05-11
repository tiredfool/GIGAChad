using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab;      // 생성에 사용할 플랫폼 프리팹
    private bool hasStepped = false;       // 중복 실행 방지
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("플레이어가 발판을 밟음");

        // 새로운 발판 위치: Y는 현재보다 3 위, X는 랜덤
        Vector3 currentPos = transform.position;
        float newX = Random.Range(269f, 280f);
        float newY = currentPos.y + 3f;
        Vector3 newPlatformPos = new Vector3(newX, newY, 0f);

        // 새로운 발판 생성
        GameObject newPlatform = Instantiate(platformPrefab, newPlatformPos, Quaternion.identity);
        newPlatform.GetComponent<PlatformTrigger>().platformPrefab = platformPrefab;

        // 현재 발판은 3초 후 제거
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }
}
