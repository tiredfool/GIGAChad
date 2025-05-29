using UnityEngine;

public class StackGameStartTrigger : MonoBehaviour
{
    public StartUpManager startUpManager; // StartUpManager ����

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (startUpManager != null)
            {
                startUpManager.gameObject.SetActive(true);
                startUpManager.ShowInstructionsAndPauseGame();
                gameObject.SetActive(false); // Ʈ���� ��Ȱ��ȭ
            }
            else
            {
                Debug.LogError("StartUpManager�� ������� �ʾҽ��ϴ�!");
            }
        }
    }
}