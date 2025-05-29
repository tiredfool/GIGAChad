using UnityEngine;

public class StackGameStartTrigger : MonoBehaviour
{
    public StartUpManager startUpManager; // StartUpManager 연결

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (startUpManager != null)
            {
                startUpManager.gameObject.SetActive(true);
                startUpManager.ShowInstructionsAndPauseGame();
                gameObject.SetActive(false); // 트리거 비활성화
            }
            else
            {
                Debug.LogError("StartUpManager가 연결되지 않았습니다!");
            }
        }
    }
}