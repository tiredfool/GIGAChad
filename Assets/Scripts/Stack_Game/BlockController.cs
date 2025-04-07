using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isStopped = false;
    private StackManager stackManager;

    public void Init(StackManager manager)
    {
        stackManager = manager;
    }

    void Update()
    {
        if (isStopped) return;

        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStopped = true;
            stackManager.OnBlockStopped(this.gameObject);
        }
    }
}
