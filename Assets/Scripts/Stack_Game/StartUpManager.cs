using UnityEngine;

public class StartUpManager : MonoBehaviour
{
    public GameObject instructionImageObject; // �ȳ� �̹��� GameObject ����
    public StackManager stackManager; // StackManager ����
    public KeyCode startGameKey = KeyCode.Q; // ���� Ű�� Q�� ����

    private bool gameStarted = false;

    void Start()
    {
        // �ȳ� �̹����� ����Ǿ����� Ȯ��
        if (instructionImageObject == null)
        {
            Debug.LogError("Instruction Image GameObject�� ������� �ʾҽ��ϴ�!");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
        }

        // StackManager�� ����Ǿ����� Ȯ��
        if (stackManager == null)
        {
            Debug.LogError("StackManager�� ������� �ʾҽ��ϴ�!");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
        }

        // ���� �� ���� �Ͻ� ����
        Time.timeScale = 0f;
        if (instructionImageObject != null)
        {
            instructionImageObject.SetActive(true);
        }
        // StackManager�� ó���� ��Ȱ��ȭ ���·� �Ӵϴ�.
        if (stackManager != null)
        {
            stackManager.gameObject.SetActive(false);
            stackManager.isStackGameActive = false; // ���� ���� Ȱ��ȭ ���µ� false�� �ʱ�ȭ
        }
    }

    void Update()
    {
        if (gameStarted) return;
        // Q Ű�� ������
        if (VirtualInputManager.Instance.GetKeyOrButton("Action") && instructionImageObject != null && instructionImageObject.activeSelf)
        {
            // �ȳ� �̹����� Ȱ��ȭ�Ǿ� �ִٸ�
            if (instructionImageObject != null && instructionImageObject.activeSelf)
            {
                // �ȳ� �̹��� ��Ȱ��ȭ
                instructionImageObject.SetActive(false);
                // ���� �ð� �ٽ� �帣�� ��
                Time.timeScale = 1f;
                // StackGame ����
                if (stackManager != null)
                {
                    stackManager.gameObject.SetActive(true);
                    if (stackManager.cameraSwitcher != null && stackManager.cameraSwitcher.stackCamera != null)
                    {
                        stackManager.StartStackGame(stackManager.cameraSwitcher.stackCamera);
                        gameStarted = true;
                    }
                    else
                    {
                        Debug.LogError("CameraSwitcher �Ǵ� Stack Camera�� StackManager�� ������� �ʾҽ��ϴ�!");
                    }
                }
            }
        }
    }

    // �ܺο��� ȣ���Ͽ� �ȳ����� Ȱ��ȭ�ϰ� ������ ���ߴ� �Լ�
    public void ShowInstructionsAndPauseGame()
    {
        if (instructionImageObject != null)
        {
            instructionImageObject.SetActive(true);
            Time.timeScale = 0f;
            // �ȳ� �߿��� StackManager ��Ȱ��ȭ �� ���� ���� ��Ȱ��ȭ
            if (stackManager != null)
            {
                stackManager.gameObject.SetActive(false);
                stackManager.isStackGameActive = false;
            }
            gameStarted = false;
        }
    }
}