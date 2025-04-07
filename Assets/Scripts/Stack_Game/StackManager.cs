using UnityEngine;
using System.Collections.Generic;

public class StackManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;

    public Camera mainCamera;
    public CameraSwitcher cameraSwitcher;

    private GameObject lastBlock;
    private float blockHeight;

    // �ʱ�ȭ ���� �ʵ�
    private Vector3 initialCameraPosition; // ī�޶� ����
    private GameObject originalBlockPrefab; // �ʱ� ���� ������
    private Vector3 nextBlockScale; // ���� ��� ũ�� �����

    // Y ���� ��ȯ�� ���� �ʵ�
    public int maxVisibleBlocks = 5;
    private List<GameObject> stackedBlocks = new List<GameObject>();
    private float totalHeightMoved = 0f;

    // ���ӿ��� ���� �ʵ�
    private bool isGameOver = false;

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void StartStackGame(Camera stackCamera)
    {
        Debug.Log("���� ���� ���۵�!");

        mainCamera = stackCamera;
        // �ʱ� ���� ī�޶� ��ġ�� ����� �� ���� ���� ����
        if (initialCameraPosition == Vector3.zero)
        {
            initialCameraPosition = mainCamera.transform.position;
        }

        originalBlockPrefab = blockPrefab;

        // Trim�� �ʿ��� ���
        //lastBlock = null;

        blockHeight = blockPrefab.transform.localScale.y;
        nextBlockScale = originalBlockPrefab.transform.localScale;

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        // �ʱ�ȭ
        ResetStackGame();
    }

    public void ResetStackGame()
    {
        // ���� ���� ���� �ʱ�ȭ
        isGameOver = false;

        // prefab �������� ����
        blockPrefab = originalBlockPrefab;

        // ���� ��� ����
        foreach (GameObject block in stackedBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }

        stackedBlocks.Clear();
        lastBlock = null;
        totalHeightMoved = 0f;

        // ī�޶� ��ġ ����
        mainCamera.transform.position = initialCameraPosition;
        nextBlockScale = originalBlockPrefab.transform.localScale;

        // ù ��� �ٽ� ����
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (isGameOver) return;

        Vector3 spawnPos = spawnPoint.position;

        if (lastBlock != null)
        {
            spawnPos.y = lastBlock.transform.position.y + blockHeight;
        }

        GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
        newBlock.transform.localScale = nextBlockScale;
        newBlock.GetComponent<BlockController>().Init(this);

        //Debug.Log("Spawned Block at: " + spawnPos);

        // ����Ʈ�� ����
        stackedBlocks.Add(newBlock);

        // ����� �ʹ� ������ ���ġ
        if (stackedBlocks.Count > maxVisibleBlocks)
        {
            RecycleBlocks();
        }
    }

    private void RecycleBlocks()
    {
        int numToRemove = stackedBlocks.Count - maxVisibleBlocks;

        // �Ʒ��� �ִ� ������ ��� ����
        for (int i = 0; i < numToRemove; i++)
        {
            Destroy(stackedBlocks[i]);
        }

        stackedBlocks.RemoveRange(0, numToRemove);

        // ������ ��ϵ� �Ʒ��� �̵�
        foreach (var block in stackedBlocks)
        {
            block.transform.position -= new Vector3(0, blockHeight * numToRemove, 0);
        }

        // ī�޶� ��ġ�� ������
        mainCamera.transform.position -= new Vector3(0, blockHeight * numToRemove, 0);

        // ������� ī�޶� �̵��� ���� ���� (���߿� �ʿ��ϸ� ��)
        totalHeightMoved += blockHeight * numToRemove;
    }


    public void OnBlockStopped(GameObject currentBlock)
    {
        if (lastBlock == null)
        {
            lastBlock = currentBlock;
            nextBlockScale = currentBlock.transform.localScale;
            SpawnBlock();
            return;
        }

        /*
         float overlap = GetOverlap(currentBlock, lastBlock, out float slicedSize, out float slicedX);

        if (overlap <= 0f)
        {
            Debug.Log("Game Over");

            if (cameraSwitcher != null)
            {
                cameraSwitcher.ExitStackMode(); // ī�޶� ����
            }
            return;
        }

        // �� �ڸ���
        float direction = Mathf.Sign(currentBlock.transform.position.x - lastBlock.transform.position.x);
        float newBlockX = lastBlock.transform.position.x + (currentBlock.transform.position.x - lastBlock.transform.position.x) / 2f;

        Vector3 trimmedScale = currentBlock.transform.localScale;
        trimmedScale.x = overlap;

        Vector3 trimmedPos = currentBlock.transform.position;
        trimmedPos.x = newBlockX;

        currentBlock.transform.localScale = trimmedScale;
        currentBlock.transform.position = trimmedPos;

        // �߸� ���� ���� (�������� �κ�)
        Vector3 slicedPos = currentBlock.transform.position + new Vector3((overlap / 2f + slicedSize / 2f) * direction, 0, 0);
        GameObject slicedPart = Instantiate(originalBlockPrefab, slicedPos, Quaternion.identity);
        slicedPart.transform.localScale = new Vector3(slicedSize, currentBlock.transform.localScale.y, 1f);
        slicedPart.AddComponent<Rigidbody2D>();
        Destroy(slicedPart, 2f);
        */

        // Trim�� ���� �������� ����
        lastBlock = currentBlock;
        nextBlockScale = currentBlock.transform.localScale;

        MoveCameraUp();
        SpawnBlock();
    }

    public void EndGame()
    {
        if (isGameOver) return; // �ߺ� ���� ����
        isGameOver = true;

        Debug.Log("Game Over");

        if (cameraSwitcher != null)
        {
            cameraSwitcher.ExitStackMode();
        }

        // ���� ��� ���߰� �ϱ�
        if (stackedBlocks.Count > 0)
        {
            GameObject current = stackedBlocks[stackedBlocks.Count - 1];
            var controller = current.GetComponent<BlockController>();
            if (controller != null)
            {
                controller.StopBlock();
            }
        }
    }

    /*
     float GetOverlap(GameObject curr, GameObject last, out float slicedSize, out float slicedX)
    {
        float currX = curr.transform.position.x;
        float lastX = last.transform.position.x;
        float currW = curr.transform.localScale.x;
        float lastW = last.transform.localScale.x;

        float distance = currX - lastX;
        float overlap = currW - Mathf.Abs(distance);
        slicedSize = Mathf.Abs(distance);
        slicedX = currX + (distance > 0 ? slicedSize / 2f : -slicedSize / 2f);
        return overlap;
    }
     */

    void MoveCameraUp()
    {
        mainCamera.transform.position += new Vector3(0, blockHeight, 0);
    }
}
