using UnityEngine;

public class StackManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;

    public Camera mainCamera;

    private GameObject lastBlock;
    private float blockHeight;

    public CameraSwitcher cameraSwitcher;

    public void StartStackGame(Camera stackCamera)
    {
        Debug.Log("스택 게임 시작됨!");

        mainCamera = stackCamera;
        lastBlock = null;
        blockHeight = blockPrefab.transform.localScale.y;

        SpawnBlock();
    }

    public void SpawnBlock()
    {
        Vector3 spawnPos = spawnPoint.position;

        if (lastBlock != null)
        {
            spawnPos.y = lastBlock.transform.position.y + blockHeight;
        }

        GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
        newBlock.GetComponent<BlockController>().Init(this);
    }

    public void OnBlockStopped(GameObject currentBlock)
    {
        if (lastBlock == null)
        {
            lastBlock = currentBlock;
            blockPrefab = currentBlock; // 다음 기준
            SpawnBlock();
            return;
        }

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
        GameObject slicedPart = Instantiate(blockPrefab, slicedPos, Quaternion.identity);
        slicedPart.transform.localScale = new Vector3(slicedSize, currentBlock.transform.localScale.y, 1f);
        slicedPart.AddComponent<Rigidbody2D>();
        Destroy(slicedPart, 2f);

        // Trim된 블럭을 기준으로 갱신
        lastBlock = currentBlock;

        // 다음 블럭도 현재 Trim된 블럭을 기반으로 생성하도록 prefab 갱신
        blockPrefab = currentBlock;

        MoveCameraUp();
        SpawnBlock();
    }


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

    void MoveCameraUp()
    {
        mainCamera.transform.position += new Vector3(0, blockHeight, 0);
    }
}
