using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchTrigger : MonoBehaviour
{
    private SpriteRenderer switchRenderer; // 스위치의 스프라이트 렌더러
    private TilemapRenderer platformRenderer; // Trick 발판의 렌더러
    private TilemapCollider2D platformCollider; // Trick 발판의 충돌 처리

    public Sprite greenLightSprite; // 녹색불 이미지
    public Sprite originalSprite; // 원래 이미지 저장
    public float activeTime = 10f; // 발판이 활성화되는 시간

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite; // 원래 이미지 저장

        // Trick 태그를 가진 발판 찾기
        GameObject platformObject = GameObject.FindGameObjectWithTag("Trick");
        if (platformObject != null)
        {
            platformRenderer = platformObject.GetComponent<TilemapRenderer>();
            platformCollider = platformObject.GetComponent<TilemapCollider2D>();

            if (platformRenderer != null) platformRenderer.enabled = false;
            if (platformCollider != null) platformCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Trick 태그를 가진 발판을 찾을 수 없습니다!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("스위치 작동: 발판 활성화 시작");
            switchRenderer.sprite = greenLightSprite; // 녹색불로 변경
            StartCoroutine(ActivatePlatform());
        }
    }

    IEnumerator ActivatePlatform()
    {
        // 스위치 투명도 변경 (0.4)
        /*
            if (switchRenderer != null)
            switchRenderer.color = new Color(1, 1, 1, 0.4f);
        */

        // 발판 활성화
        if (platformRenderer != null) platformRenderer.enabled = true;
        if (platformCollider != null) platformCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        // 발판 비활성화
        if (platformRenderer != null) platformRenderer.enabled = false;
        if (platformCollider != null) platformCollider.enabled = false;

        // 스위치 투명도 원래대로 (1.0)

        if (switchRenderer != null)
        {          
            switchRenderer.sprite = originalSprite; // 원래 이미지로 복구
        }


        Debug.Log("발판 비활성화됨");
    }
}
