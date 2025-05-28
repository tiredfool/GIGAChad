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
            stackManager.isStackGameActive = true;
            // 플레이어 조작 제한 (선택 사항)
            if (playerController != null)
            {
                playerController.SetStackGameMode(true);
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
