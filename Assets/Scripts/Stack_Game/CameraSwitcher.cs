using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera playerCamera;
    public Camera stackCamera;
    public StackManager stackManager; // StackManager 연결 필수!

    private bool isInStackMode = false;
    private PlayerController playerController;

    private void Start()
    {
        stackCamera.enabled = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 카메라 전환
            playerCamera.enabled = false;
            stackCamera.enabled = true;
            isInStackMode = true;

            // 플레이어 조작 제한 (선택 사항)
            if (playerController != null)
            {
                playerController.SetStackGameMode(true);
            }

            // 스택 게임 시작
            if (stackManager != null)
            {
                stackManager.StartStackGame(stackCamera); // 카메라 넘겨주기
                stackManager.isGameStart = true;
            }
            else
            {
                Debug.LogWarning("StackManager가 연결되지 않았습니다!");
            }
        }
    }

    public void ExitStackMode()
    {
        stackCamera.enabled = false;
        playerCamera.enabled = true;
        isInStackMode = false;

        if (playerController != null)
        {
            playerController.SetStackGameMode(false);
        }
    }
}
