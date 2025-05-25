using UnityEngine;

public class StartUpManager : MonoBehaviour
{
    public GameObject instructionImageObject; // 안내 이미지 GameObject 연결
    public StackManager stackManager; // StackManager 연결
    public KeyCode startGameKey = KeyCode.Q; // 시작 키를 Q로 변경

    private bool gameStarted = false;

    void Start()
    {
        // 안내 이미지가 연결되었는지 확인
        if (instructionImageObject == null)
        {
            Debug.LogError("Instruction Image GameObject가 연결되지 않았습니다!");
            enabled = false; // 스크립트 비활성화
        }

        // StackManager가 연결되었는지 확인
        if (stackManager == null)
        {
            Debug.LogError("StackManager가 연결되지 않았습니다!");
            enabled = false; // 스크립트 비활성화
        }

        // 시작 시 게임 일시 정지
        Time.timeScale = 0f;
        if (instructionImageObject != null)
        {
            instructionImageObject.SetActive(true);
        }
        // StackManager는 처음에 비활성화 상태로 둡니다.
        if (stackManager != null)
        {
            stackManager.gameObject.SetActive(false);
            stackManager.isStackGameActive = false; // 스택 게임 활성화 상태도 false로 초기화
        }
    }

    void Update()
    {
        if (gameStarted) return;
        // Q 키가 눌리면
        if (VirtualInputManager.Instance.GetKeyOrButton("Action") && instructionImageObject != null && instructionImageObject.activeSelf)
        {
            // 안내 이미지가 활성화되어 있다면
            if (instructionImageObject != null && instructionImageObject.activeSelf)
            {
                // 안내 이미지 비활성화
                instructionImageObject.SetActive(false);
                // 게임 시간 다시 흐르게 함
                Time.timeScale = 1f;
                // StackGame 시작
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
                        Debug.LogError("CameraSwitcher 또는 Stack Camera가 StackManager에 연결되지 않았습니다!");
                    }
                }
            }
        }
    }

    // 외부에서 호출하여 안내문을 활성화하고 게임을 멈추는 함수
    public void ShowInstructionsAndPauseGame()
    {
        if (instructionImageObject != null)
        {
            instructionImageObject.SetActive(true);
            Time.timeScale = 0f;
            // 안내 중에는 StackManager 비활성화 및 스택 게임 비활성화
            if (stackManager != null)
            {
                stackManager.gameObject.SetActive(false);
                stackManager.isStackGameActive = false;
            }
            gameStarted = false;
        }
    }
}