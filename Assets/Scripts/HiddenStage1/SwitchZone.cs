using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchZone : MonoBehaviour
{
    public GameObject platformerPlayer;
    public GameObject topDownPlayer;
    public CinemachineVirtualCamera vcamPlatformer;
    public CinemachineVirtualCamera vcamTopDown;
    public float switchDuration = 10f;
    public GameObject foodSpawner;

    private bool isTopDown = false;

    private void Start()
    {
        foodSpawner.SetActive(false);
        topDownPlayer.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == platformerPlayer && !isTopDown)
        {
            StartCoroutine(SwitchToTopDown());
        }
    }

    private IEnumerator SwitchToTopDown()
    {
        isTopDown = true;

        vcamPlatformer.Priority = 5;
        vcamTopDown.Priority = 10;

        // 톱다운 모드 활성화
        foodSpawner.SetActive(true);
        topDownPlayer.SetActive(true);
        platformerPlayer.SetActive(false);

        Debug.Log("톱다운 모드 시작");

        //10초 동안 톱다운 모드 유지
        yield return new WaitForSeconds(switchDuration);

        // 시간이 지나면 톱다운 관련 오브젝트 삭제
        Debug.Log(" 톱다운 모드 종료");
        Destroy(topDownPlayer);
        Destroy(foodSpawner);
        Destroy(vcamTopDown);

        //  플랫포머 모드로 돌아가기
        StartCoroutine(SwitchToPlatformer());
        Destroy(gameObject);
    }

    private IEnumerator SwitchToPlatformer()
    {
        Debug.Log(" 플랫포머 모드로 복귀");

        isTopDown = false;

        vcamPlatformer.Priority = 10;

        // ✅ 플랫포머 플레이어 다시 활성화
        platformerPlayer.SetActive(true);
        Debug.Log("플랫포머 플레이어 활성화");

        yield return null;
    }
}
