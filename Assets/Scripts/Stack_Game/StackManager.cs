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

    // 초기화 관련 필드
    private Vector3 initialCameraPosition; // 카메라 시점
    private GameObject originalBlockPrefab; // 초기 상태 보존용
    private Vector3 nextBlockScale; // 다음 블록 크기 저장용

    // Y 기준 순환을 위한 필드
    public int maxVisibleBlocks = 5;
    private List<GameObject> stackedBlocks = new List<GameObject>();
    private float totalHeightMoved = 0f;

    // 게임오버 관련 필드
    private bool isGameOver = false;

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void StartStackGame(Camera stackCamera)
    {
        Debug.Log("스택 게임 시작됨!");

        mainCamera = stackCamera;
        // 초기 시작 카메라 위치가 저장된 적 없을 때만 저장
        if (initialCameraPosition == Vector3.zero)
        {
            initialCameraPosition = mainCamera.transform.position;
        }

        originalBlockPrefab = blockPrefab;

        // Trim에 필요한 요소
        //lastBlock = null;

        blockHeight = blockPrefab.transform.localScale.y;
        nextBlockScale = originalBlockPrefab.transform.localScale;

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        // 초기화
        ResetStackGame();
    }

    public void ResetStackGame()
    {
        // 게임 오버 상태 초기화
        isGameOver = false;

        // prefab 원본으로 복원
        blockPrefab = originalBlockPrefab;

        // 기존 블록 제거
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

        // 카메라 위치 복구
        mainCamera.transform.position = initialCameraPosition;
        nextBlockScale = originalBlockPrefab.transform.localScale;

        // 첫 블록 다시 스폰
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

        // 리스트에 저장
        stackedBlocks.Add(newBlock);

        // 블록이 너무 많으면 재배치
        if (stackedBlocks.Count > maxVisibleBlocks)
        {
            RecycleBlocks();
        }
    }

    private void RecycleBlocks()
    {
        int numToRemove = stackedBlocks.Count - maxVisibleBlocks;

        // 아래에 있는 오래된 블록 제거
        for (int i = 0; i < numToRemove; i++)
        {
            Destroy(stackedBlocks[i]);
        }

        stackedBlocks.RemoveRange(0, numToRemove);

        // 나머지 블록들 아래로 이동
        foreach (var block in stackedBlocks)
        {
            block.transform.position -= new Vector3(0, blockHeight * numToRemove, 0);
        }

        // 카메라 위치도 재조정
        mainCamera.transform.position -= new Vector3(0, blockHeight * numToRemove, 0);

        // 현재까지 카메라가 이동한 높이 저장 (나중에 필요하면 씀)
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
                cameraSwitcher.ExitStackMode(); // 카메라 복귀
            }
            return;
        }

        // 블럭 자르기
        float direction = Mathf.Sign(currentBlock.transform.position.x - lastBlock.transform.position.x);
        float newBlockX = lastBlock.transform.position.x + (currentBlock.transform.position.x - lastBlock.transform.position.x) / 2f;

        Vector3 trimmedScale = currentBlock.transform.localScale;
        trimmedScale.x = overlap;

        Vector3 trimmedPos = currentBlock.transform.position;
        trimmedPos.x = newBlockX;

        currentBlock.transform.localScale = trimmedScale;
        currentBlock.transform.position = trimmedPos;

        // 잘린 조각 생성 (떨어지는 부분)
        Vector3 slicedPos = currentBlock.transform.position + new Vector3((overlap / 2f + slicedSize / 2f) * direction, 0, 0);
        GameObject slicedPart = Instantiate(originalBlockPrefab, slicedPos, Quaternion.identity);
        slicedPart.transform.localScale = new Vector3(slicedSize, currentBlock.transform.localScale.y, 1f);
        slicedPart.AddComponent<Rigidbody2D>();
        Destroy(slicedPart, 2f);
        */

        // Trim된 블럭을 기준으로 갱신
        lastBlock = currentBlock;
        nextBlockScale = currentBlock.transform.localScale;

        MoveCameraUp();
        SpawnBlock();
    }

    public void EndGame()
    {
        if (isGameOver) return; // 중복 종료 방지
        isGameOver = true;

        Debug.Log("Game Over");

        if (cameraSwitcher != null)
        {
            cameraSwitcher.ExitStackMode();
        }

        // 현재 블록 멈추게 하기
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
