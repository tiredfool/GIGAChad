using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchMoveToRight : MonoBehaviour
{
    private SpriteRenderer switchRenderer; // 스위치의 스프라이트 렌더러
    public Sprite greenLightSprite; // 녹색불 이미지
    public Sprite originalSprite; // 원래 이미지 저장
    public float moveDistance = 10f; // 이동 거리 (우측으로 10칸)
    public float moveSpeed = 2f; // 이동 속도

    private Transform movePlatformTransform; // 이동할 발판의 Transform
    private Vector3 targetPosition; // 이동 목표 지점
    private bool isMoving = false; // 이동 중 여부 체크

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite; // 원래 이미지 저장

        // "Move" 태그를 가진 발판 찾기
        GameObject movePlatform = GameObject.FindGameObjectWithTag("Move");
        if (movePlatform != null)
        {
            movePlatformTransform = movePlatform.transform;
            targetPosition = movePlatformTransform.position + new Vector3(moveDistance, 0, 0); // 우측으로 10칸 이동
        }
        else
        {
            Debug.LogError("Move 태그를 가진 발판을 찾을 수 없습니다!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMoving)
        {
            Debug.Log("스위치 작동: 발판 이동 시작");
            switchRenderer.sprite = greenLightSprite; // 녹색불로 변경
            StartCoroutine(MovePlatform());
        }
    }

    IEnumerator MovePlatform()
    {
        isMoving = true;

        while (Vector3.Distance(movePlatformTransform.position, targetPosition) > 0.01f)
        {
            movePlatformTransform.position = Vector3.MoveTowards(movePlatformTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // 한 프레임 대기
        }

        movePlatformTransform.position = targetPosition; // 정확한 위치 고정
        isMoving = false;

        Debug.Log("발판 이동 완료");
    }
}
