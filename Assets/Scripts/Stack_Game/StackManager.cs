 using UnityEngine;
using System.Collections; // �ڷ�ƾ ����� ���� �߰�
using System.Collections.Generic;

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
    public GameObject badgeImageObject; // Inspector���� ������ ���� GameObject
    public Sprite[] badgeSprites; // Inspector���� ������ ���� Sprite �迭
    public Vector3 badgeOffset = new Vector3(2f, 2f, 0f); // ī�޶�κ����� ������� ��ġ ������
    // Studying
    public GameObject studyingImageObject;
    public Vector3 studyingOffset = new Vector3(0f, 0f, 10f);

    private bool canExit = false; // �� �̻� ������� ����
    public float exitDelay = 3f; // 3�� ������
    public PlayerController Pc;
    public bool IsGameOver() { return isGameOver; }

    public void StartStackGame(Camera stackCamera)
    {
        DialogueManager.instance.StartDialogueByIdRange("2S-m-1", "2S-m-2");
        Debug.Log("���� ���� ���۵�!");
        isStackGameActive = true;
        mainCamera = stackCamera;
        if (initialCameraPosition == Vector3.zero) initialCameraPosition = mainCamera.transform.position;
        originalBlockPrefab = blockPrefab;
        blockHeight = blockPrefab.transform.localScale.y;
        nextBlockScale = originalBlockPrefab.transform.localScale;
        if (mainCamera != null) mainCamera.enabled = true;

        MainSoundManager.instance.ChangeBGM("StackGame");

        ResetStackGame();
        // ���� ���� �� ���� GameObject ��Ȱ��ȭ
        if (badgeImageObject != null)
        {
            badgeImageObject.SetActive(false);
        }
        if (studyingImageObject != null)
        {
            studyingImageObject.SetActive(true);
            UpdateStudyingImagePosition(); // �ʱ� ��ġ ����
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
        // ���� ����� �� ���� GameObject ��Ȱ��ȭ
        if (badgeImageObject != null)
        {
            badgeImageObject.SetActive(false);
        }
        if (studyingImageObject != null)
        {
            studyingImageObject.SetActive(true);
            UpdateStudyingImagePosition(); // �ʱ� ��ġ ����
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
                Debug.Log("�ڰ����� ����߽��ϴ�!!!");
                ShowBadge(licenseCount); // ���� ǥ��
                licenseCount++;
            }
            if (licenseCount >= maxLicenses)
            {
                Debug.Log("��� �ڰ����� ����߽��ϴ�! " + exitDelay + "�� �Ŀ� ���� �������� ���ư��ϴ�.");
                StartCoroutine(ReturnToOriginalGame()); // 3�� �� ���ư��� �ڷ�ƾ ����
                return; // �� �̻� ����� �������� �ʵ��� return
            }
        }

        lastBlock = currentBlock;
        SpriteRenderer renderer = lastBlock.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder = baseBlockOrderInLayer + stackedBlocks.Count;

        MoveCameraUp();
        UpdateBadgePosition(); // �� ������ ���� ��ġ ������Ʈ

        nextBlockScale = currentBlock.transform.localScale;
        SpawnBlock();
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
                Debug.LogError("BadgeImage GameObject�� SpriteRenderer�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogWarning("���� GameObject �Ǵ� ��������Ʈ �迭�� ������� �ʾҰų� �ε����� �߸��Ǿ����ϴ�.");
        }
    }

    void UpdateBadgePosition()
    {
        if (badgeImageObject != null && mainCamera != null)
        {
            badgeImageObject.transform.position = mainCamera.transform.position + badgeOffset;
        }
    }

    // Studying �̹����� ��ġ�� ������Ʈ�ϴ� �� �޼���
    void UpdateStudyingImagePosition()
    {
        if (studyingImageObject != null && mainCamera != null)
        {
            // Studying ������Ʈ�� ī�޶��� ��ġ�� �������� ���Ͽ� ����
            studyingImageObject.transform.position = mainCamera.transform.position + studyingOffset;
        }
    }

    IEnumerator ReturnToOriginalGame()
    {
        yield return new WaitForSeconds(exitDelay);
        EndGame();
    }

    public void EndGame()
    {
        if (!isStackGameActive || isGameOver) return;

        isGameOver = true;
        isStackGameActive = false;
        Debug.Log("Game Over");
        Pc.SetStackGameMode(false);
        if (cameraSwitcher != null)
        {
            cameraSwitcher.ExitStackMode();
            cameraSwitcher.gameObject.SetActive(false); // CameraSwitcher�� ��Ȱ��ȭ
        }
        if (stackedBlocks.Count > 0)
        {
            GameObject current = stackedBlocks[stackedBlocks.Count - 1];
            var controller = current.GetComponent<BlockController>();
            if (controller != null) controller.StopBlock();
        }
        MainSoundManager.instance.ChangeBGM("2stage");
    }

    void MoveCameraUp()
    {
        if (isStackGameActive) mainCamera.transform.position += new Vector3(0, blockHeight + 0.5f, 0);
        UpdateStudyingImagePosition();
    }
}