using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactStartGame : MonoBehaviour
{
    [Tooltip("GameManager 스크립트가 있는 오브젝트")]
    public BossManager bossManager;

    [Tooltip("인식할 플레이어 태그")]
    public string playerTag = "Player";

    private bool hasContacted = false; // 한 번만 시작되도록 체크

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasContacted && other.CompareTag(playerTag))
        {
            if (bossManager != null)
            {
                bossManager.StartAllFeatures();
                Debug.Log(gameObject.name + ": 플레이어와 접촉하여 게임 시작!");
                hasContacted = true; // 한 번 접촉 후에는 다시 시작되지 않도록 설정
                                     // (선택 사항) 이 트리거 컴포넌트를 더 이상 필요 없으면 비활성화하거나 제거할 수 있습니다.
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false; // 비활성화
                                         // Destroy(col); // 제거 (주의: 다른 스크립트에서 참조할 수 있음)
                }
            }
            else
            {
                Debug.LogError(gameObject.name + ": GameManager가 연결되지 않았습니다!");
            }
        }
    }

   
}