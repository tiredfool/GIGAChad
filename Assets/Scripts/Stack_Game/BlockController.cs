using UnityEngine;
using System.Collections;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isStopped = false;
    private StackManager stackManager;

    private bool canBeDetected = false;

    private SpriteRenderer spriteRenderer;
    public Sprite[] blockSprites; // ����� ��������Ʈ �迭 (Inspector���� �Ҵ�)

    public float targetBlockWidth = 1f; // ��ǥ ��� ���� ũ��

    public void Init(StackManager manager)
    {
        stackManager = manager;
        isStopped = false;

        // SpriteRenderer ������Ʈ ��������
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
        yield return new WaitForSeconds(0.5f); // 0.5�� �ں��� �浹 ���� ���
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
            Debug.Log("����� ��ü������ �������� �Ѿ����ϴ�. ���� ����!");
            stackManager.EndGame();
        }
    }

    // RightLimit �±׿� �浹 �� ���� ���� ó��
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RightLimit"))
        {
            Debug.Log("����� RightLimit�� ��ҽ��ϴ�. ���� ����!");
            stackManager.EndGame();
        }
    }

}
