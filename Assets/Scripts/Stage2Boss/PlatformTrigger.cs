using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platformPrefab; // Platform 프리팹
    private bool hasTriggered = false; // 중복 방지용 플래그

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return; // 이미 실행됐다면 무시
        if (!other.CompareTag("Player")) return;

        Debug.Log("플레이어가 발판 트리거에 닿음");

        hasTriggered = true; // 첫 실행 이후 다시는 실행 안됨

        // 다음 발판 생성 시도
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        Debug.Log("발판 개수: " + platforms.Length);

        if (platforms.Length == 0) return;

        GameObject chosen = platforms[Random.Range(0, platforms.Length)];
        Vector3 spawnPos = chosen.transform.position + new Vector3(0, 3, 0);

        // 중복 위치 방지: 같은 Y에 이미 있는지 확인
        foreach (GameObject p in platforms)
        {
            if (Mathf.Approximately(p.transform.position.y, spawnPos.y))
            {
                Debug.Log("같은 높이에 이미 발판이 있어 생성 생략");
                return;
            }
        }

        Debug.Log("새 발판 생성 위치: " + spawnPos);

        Instantiate(platformPrefab, spawnPos, Quaternion.identity);

        // Tilemap 깜빡이기 시작
        StartCoroutine(FlickerAndDestroy());
    }

    IEnumerator FlickerAndDestroy()
    {
        TilemapRenderer renderer = GetComponentInParent<TilemapRenderer>();
        GameObject toDestroy = transform.parent.gameObject;

        if (renderer == null)
        {
            Debug.LogWarning("TilemapRenderer가 부모 오브젝트에 없습니다.");
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

        Destroy(toDestroy); // 부모 오브젝트 제거
    }

}
