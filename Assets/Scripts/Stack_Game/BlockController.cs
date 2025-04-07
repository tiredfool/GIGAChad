using UnityEngine;
using System.Collections;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isStopped = false;
    private StackManager stackManager;

    private bool canBeDetected = false;

    public void Init(StackManager manager)
    {
        stackManager = manager;
        isStopped = false;
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
