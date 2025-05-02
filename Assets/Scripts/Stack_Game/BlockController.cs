using UnityEngine;
using System.Collections;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isStopped = false;
    private StackManager stackManager;

    private bool canBeDetected = false;

    private SpriteRenderer spriteRenderer;
    public Sprite[] blockSprites; // 사용할 스프라이트 배열 (Inspector에서 할당)

    public float targetBlockWidth = 1f; // 목표 블록 가로 크기

    public void Init(StackManager manager)
    {
        stackManager = manager;
        isStopped = false;

        // SpriteRenderer 컴포넌트 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && blockSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, blockSprites.Length);
            spriteRenderer.sprite = blockSprites[randomIndex];

            Sprite currentSprite = spriteRenderer.sprite;
            if (currentSprite != null && currentSprite.rect.width > 0)
            {
                float spriteWidthPixels = currentSprite.rect.width;
                float spriteHeightPixels = currentSprite.rect.height;
                float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;

                float spriteWidthUnits = spriteWidthPixels / pixelsPerUnit;
                float spriteHeightUnits = spriteHeightPixels / pixelsPerUnit;

                float scaleFactorX = targetBlockWidth / spriteWidthUnits;
                float scaleFactorY = targetBlockWidth / spriteWidthUnits * (spriteHeightUnits / spriteWidthUnits);

                transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
            }
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f); // 0.5초 뒤부터 충돌 감지 허용
        canBeDetected = true;
    }

    public bool CanBeDetected()
    {
        return canBeDetected;
    }

    public void StopBlock()
    {
        isStopped = true;
        CheckOutOfBounds();
    }

    void Update()
    {
        if (isStopped || stackManager.IsGameOver())
            return;

        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStopped = true;
            stackManager.OnBlockStopped(this.gameObject);
            CheckOutOfBounds();
        }
    }

    private void CheckOutOfBounds()
    {
        Bounds blockBounds = GetComponent<Collider2D>().bounds;

        GameObject leftBoundary = GameObject.FindGameObjectWithTag("LeftBoundary");
        GameObject rightBoundary = GameObject.FindGameObjectWithTag("RightBoundary");

        float leftLimit = leftBoundary.transform.position.x;
        float rightLimit = rightBoundary.transform.position.x;

        if (blockBounds.min.x < leftLimit || blockBounds.max.x > rightLimit)
        {
            Debug.Log("블록이 전체적으로 판정선을 넘었습니다. 게임 종료!");
            stackManager.EndGame();
        }
    }

    // RightLimit 태그와 충돌 시 게임 종료 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RightLimit"))
        {
            Debug.Log("블록이 RightLimit에 닿았습니다. 게임 종료!");
            stackManager.EndGame();
        }
    }

}
