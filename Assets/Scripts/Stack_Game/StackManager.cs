using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StackManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public Camera mainCamera;
    public CameraSwitcher cameraSwitcher;
    private GameObject lastBlock;
    private float blockHeight;
    private Vector3 initialCameraPosition;
    private GameObject originalBlockPrefab;
    private Vector3 nextBlockScale;
    public int maxVisibleBlocks = 5;
    private List<GameObject> stackedBlocks = new List<GameObject>();
    private float totalHeightMoved = 0f;
    private bool isGameOver = false;
    private int blocksStackedCount = 0;
    private int licenseCount = 0;
    public float blockSpeedIncrement = 1f;
    public int licenseThreshold = 10;
    public int maxLicenses = 3;
    public bool isStackGameActive = false;
    public string blockSortingLayerName = "Default";
    public int baseBlockOrderInLayer = 0;

    // Badge
    public GameObject badgeImageObject; // Inspector에서 연결할 뱃지 GameObject
    public Sprite[] badgeSprites; // Inspector에서 연결할 뱃지 Sprite 배열
    public Vector3 badgeOffset = new Vector3(2f, 2f, 0f); // 카메라로부터의 상대적인 위치 오프셋

    public bool AllLicensesObtained { get; private set; } = false; // 영구적인 모든 자격증 취득 여부
    public bool WasLastGameSuccessful { get; private set; } = false; // 직전 스택 게임 세션의 성공 여부

    // Studying
    public GameObject studyingImageObject;
    public Vector3 studyingOffset = new Vector3(0f, 0f, 10f);

    public float exitDelay = 3f; // 3초 딜레이
    public PlayerController Pc;
    public bool IsGameOver() { return isGameOver; }

    void Awake()
    {
        LoadLicenseStatus();
    }

    void OnApplicationQuit()
    {
        SaveLicenseStatus();
    }

    public void StartStackGame(Camera stackCamera)
    {
        DialogueManager.instance.StartDialogueByIdRange("2S-m-1", "2S-m-2");
        Debug.Log("스택 게임 시작됨!");
        isStackGameActive = true;
        mainCamera = stackCamera;

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        if (initialCameraPosition == Vector3.zero) initialCameraPosition = mainCamera.transform.position;
        originalBlockPrefab = blockPrefab;
        blockHeight = blockPrefab.transform.localScale.y;
        nextBlockScale = originalBlockPrefab.transform.localScale;
        if (mainCamera != null) mainCamera.enabled = true;

        MainSoundManager.instance.ChangeBGM("StackGame");

        WasLastGameSuccessful = false; // 새로운 스택 게임 시작 시 초기화

        ResetStackGame();
        // 게임 시작 시 뱃지 GameObject 비활성화
        if (badgeImageObject != null)
        {
            badgeImageObject.SetActive(false);
        }
        if (studyingImageObject != null)
        {
            studyingImageObject.SetActive(true);
            UpdateStudyingImagePosition(); // 초기 위치 설정
        }
    }

    public void ResetStackGame()
    {
        isGameOver = false;
        blocksStackedCount = 0;
        licenseCount = 0;

        blockPrefab = originalBlockPrefab;
        foreach (GameObject block in stackedBlocks) if (block != null) Destroy(block);
        stackedBlocks.Clear();
        lastBlock = null;
        totalHeightMoved = 0f;
        mainCamera.transform.position = initialCameraPosition;
        nextBlockScale = originalBlockPrefab.transform.localScale;
        if (isStackGameActive) SpawnBlock();
        // 게임 재시작 시 뱃지 GameObject 비활성화
        if (badgeImageObject != null)
        {
            badgeImageObject.SetActive(false);
        }
        if (studyingImageObject != null)
        {
            studyingImageObject.SetActive(true);
            UpdateStudyingImagePosition(); // 초기 위치 설정
        }
    }

    public void SpawnBlock()
    {
        if (!isStackGameActive || isGameOver) return;

        Vector3 spawnPos = spawnPoint.position;
        if (lastBlock != null) spawnPos.y = lastBlock.transform.position.y + blockHeight + 0.5f;
        GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
        newBlock.transform.localScale = nextBlockScale;
        BlockController blockController = newBlock.GetComponent<BlockController>();
        blockController.Init(this);
        blockController.moveSpeed += licenseCount * blockSpeedIncrement;

        SpriteRenderer renderer = newBlock.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = blockSortingLayerName;
            renderer.sortingOrder = baseBlockOrderInLayer + stackedBlocks.Count;
        }

        stackedBlocks.Add(newBlock);
        if (stackedBlocks.Count > maxVisibleBlocks) RecycleBlocks();

        lastBlock = newBlock;
    }

    private void RecycleBlocks()
    {
        int numToRemove = stackedBlocks.Count - maxVisibleBlocks;
        for (int i = 0; i < numToRemove; i++)
        {
            GameObject blockToRemove = stackedBlocks[i];
            if (blockToRemove != null) Destroy(blockToRemove);
        }
        stackedBlocks.RemoveRange(0, numToRemove);

        for (int i = 0; i < stackedBlocks.Count; i++)
        {
            SpriteRenderer renderer = stackedBlocks[i].GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.sortingOrder = baseBlockOrderInLayer + i;
        }

        foreach (var block in stackedBlocks) block.transform.position -= new Vector3(0, blockHeight * numToRemove + 0.5f * numToRemove, 0);
        mainCamera.transform.position -= new Vector3(0, blockHeight * numToRemove + 0.5f * numToRemove, 0);
        totalHeightMoved += blockHeight * numToRemove + 0.5f * numToRemove;
    }

    public void OnBlockStopped(GameObject currentBlock)
    {
        if (!isStackGameActive || isGameOver) return;

        blocksStackedCount++;
        if (blocksStackedCount % licenseThreshold == 0)
        {
            if (licenseCount < maxLicenses)
            {
                //Debug.Log("자격증을 취득했습니다!!!");
                ShowBadge(licenseCount); // 뱃지 표시
                licenseCount++;
            }
            if (licenseCount >= maxLicenses)
            {
                //Debug.Log("모든 자격증을 취득했습니다! " + exitDelay + "초 후에 원래 게임으로 돌아갑니다.");
                AllLicensesObtained = true;
                SaveLicenseStatus();

                // GameManager의 현재 게임 실행 상태 업데이트
                if (GameManager.instance != null) // 'instance'로 접근
                {
                    GameManager.instance.HasStackGameSucceededThisRun = true;
                    Debug.Log("StackManager: 현재 게임 실행에서 스택 게임 성공으로 GameManager 상태 업데이트!");
                }
                else
                {
                    Debug.LogWarning("StackManager: GameManager.instance를 찾을 수 없습니다! 스택 게임 성공 상태를 저장할 수 없습니다.");
                }

                StartCoroutine(ReturnToOriginalGame(true)); // 3초 후 돌아가는 코루틴 시작
                return; // 더 이상 블록을 생성하지 않도록 return
            }
        }

        lastBlock = currentBlock;
        SpriteRenderer renderer = lastBlock.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder = baseBlockOrderInLayer + stackedBlocks.Count;

        MoveCameraUp();
        UpdateBadgePosition(); // 매 프레임 뱃지 위치 업데이트

        nextBlockScale = currentBlock.transform.localScale;
        SpawnBlock();
    }

    private void SaveLicenseStatus()
    {
        PlayerPrefs.SetInt("AllLicensesObtained", AllLicensesObtained ? 1 : 0);
        PlayerPrefs.Save(); // 변경사항을 즉시 저장
        Debug.Log("License Status Saved: " + AllLicensesObtained);
    }
    private void LoadLicenseStatus()
    {
        AllLicensesObtained = PlayerPrefs.GetInt("AllLicensesObtained", 0) == 1; // 기본값은 false (0)
        Debug.Log("License Status Loaded: " + AllLicensesObtained);
    }

    void ShowBadge(int badgeIndex)
    {
        if (badgeImageObject != null && badgeSprites != null && badgeIndex < badgeSprites.Length)
        {
            SpriteRenderer badgeRenderer = badgeImageObject.GetComponent<SpriteRenderer>();
            if (badgeRenderer != null)
            {
                badgeRenderer.sprite = badgeSprites[badgeIndex];
                badgeImageObject.SetActive(true);
            }
            else
            {
                Debug.LogError("BadgeImage GameObject에 SpriteRenderer가 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("뱃지 GameObject 또는 스프라이트 배열이 연결되지 않았거나 인덱스가 잘못되었습니다.");
        }
    }

    void UpdateBadgePosition()
    {
        if (badgeImageObject != null && mainCamera != null)
        {
            badgeImageObject.transform.position = mainCamera.transform.position + badgeOffset;
        }
    }

    // Studying 이미지의 위치를 업데이트하는 새 메서드
    void UpdateStudyingImagePosition()
    {
        if (studyingImageObject != null && mainCamera != null)
        {
            // Studying 오브젝트를 카메라의 위치에 오프셋을 더하여 설정
            studyingImageObject.transform.position = mainCamera.transform.position + studyingOffset;
        }
    }

    IEnumerator ReturnToOriginalGame(bool allLicensesObtained)
    {
        yield return new WaitForSeconds(exitDelay);
        EndGame(allLicensesObtained);
    }

    public void EndGame(bool allLicensesObtainedInEndGame = false)
    {
        if (!isStackGameActive || isGameOver) return;

        isGameOver = true;
        isStackGameActive = false;
        Debug.Log("Game Over");
        Pc.SetStackGameMode(false);
        if (cameraSwitcher != null)
        {
            cameraSwitcher.ExitStackMode();
            cameraSwitcher.gameObject.SetActive(false); // CameraSwitcher를 비활성화
        }
        if (stackedBlocks.Count > 0)
        {
            GameObject current = stackedBlocks[stackedBlocks.Count - 1];
            var controller = current.GetComponent<BlockController>();
            if (controller != null) controller.StopBlock();
        }
        MainSoundManager.instance.ChangeBGM("2stage");

        WasLastGameSuccessful = allLicensesObtainedInEndGame;

        Debug.Log($"EndGame 호출됨. 현재 AllLicensesObtained 상태 (속성): {this.AllLicensesObtained}");
        Debug.Log($"EndGame 호출 시 allLicensesObtainedInEndGame 파라미터 값: {allLicensesObtainedInEndGame}");
        Debug.Log($"EndGame 호출 시 WasLastGameSuccessful (새 속성): {WasLastGameSuccessful}");

        if (allLicensesObtainedInEndGame)
        {
            DialogueManager.instance.StartDialogueByIdRange("2S-m-3", "2S-m-4");
        }
        else
        {
            DialogueManager.instance.StartDialogueByIdRange("2S-m-5", "2S-m-6");
        }
    }

    void MoveCameraUp()
    {
        if (isStackGameActive) mainCamera.transform.position += new Vector3(0, blockHeight + 0.5f, 0);
        UpdateStudyingImagePosition();
    }
}