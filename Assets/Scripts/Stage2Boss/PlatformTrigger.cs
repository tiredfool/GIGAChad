using UnityEngine;
using System.Collections;

public class PlatformTrigger : MonoBehaviour
{
    private bool hasStepped = false;
    private float destroyDelay = 3f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStepped || !collision.collider.CompareTag("Player")) return;

        hasStepped = true;
        Debug.Log("플레이어가 플랫폼에 닿음");

        // Stage2Manager에 이벤트 전달
        if (Stage2Manager.instance != null)
        {
            Stage2Manager.instance.HandlePlatformStepped(transform.position);
        }

        // 일정 시간 후 자신 삭제
        StartCoroutine(DestroyAfterDelay(gameObject));
    }

    IEnumerator DestroyAfterDelay(GameObject platform)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(platform);
    }
}
