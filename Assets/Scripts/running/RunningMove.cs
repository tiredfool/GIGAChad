using UnityEngine;
using UnityEngine.Tilemaps;

public class RunningMove : MonoBehaviour
{
    [Tooltip("�ؽ�ó ��ũ�� �ӵ� (X, Y ����)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // �������� ��ũ��

     

    [Header("Physics Materials for Conveyor")] // 2D ���� ��Ƽ������ ���� ���
    [Tooltip("�����̾� ��Ʈ�� Ȱ��ȭ(������) ������ �� ������ Physics Material 2D (���� ����)")]
    public PhysicsMaterial2D activePhysicsMaterial;
    [Tooltip("�����̾� ��Ʈ�� ��Ȱ��ȭ(����) ������ �� ������ Physics Material 2D (���� ����)")]
    public PhysicsMaterial2D inactivePhysicsMaterial;

    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider; // TilemapCollider2D ����
    private bool isScrolling = false; // ��ũ�� ���¸� �����ϴ� ����

    void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
           // Debug.LogError("RunningMove: TilemapRenderer�� ã�� �� �����ϴ�! �� ��ũ��Ʈ�� TilemapRenderer�� �ִ� ������Ʈ�� �����Ǿ�� �մϴ�.", this);
            enabled = false; // ������Ʈ ��Ȱ��ȭ
            return;
        }

       

        tilemapCollider = GetComponent<TilemapCollider2D>(); // TilemapCollider2D ����
        if (tilemapCollider == null)
        {
          //  Debug.LogError("RunningMove: TilemapCollider2D�� ã�� �� �����ϴ�! ���� ��Ƽ������ ������ �� �����ϴ�.", this);
        
        }

        // ��ũ��Ʈ ���� �� ���� ��Ƽ���� ��Ȱ�� ���·� ����
        ApplyPhysicsMaterial(inactivePhysicsMaterial);
    }

    void Update()
    {
      
        if (isScrolling && tilemapRenderer.material != null)
        {
            Vector2 currentOffset = tilemapRenderer.material.mainTextureOffset;
            currentOffset += scrollSpeed * Time.deltaTime;
            currentOffset.x = currentOffset.x % 1.0f;
            currentOffset.y = currentOffset.y % 1.0f;
            tilemapRenderer.material.mainTextureOffset = currentOffset;
        }
    }


    public void StartScrolling()
    {
        if (isScrolling) return; // �̹� ��ũ�� ���̸� �ߺ� ȣ�� ����

        isScrolling = true;

        ApplyPhysicsMaterial(activePhysicsMaterial);

        // ���� ���
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.PlaySFX("Running");

        Debug.Log(gameObject.name + ": �����̾� Ȱ��ȭ�� (���� ��Ƽ���� ����).");
    }


    public void StopScrolling()
    {
        if (!isScrolling) return; // �̹� ���������� �ߺ� ȣ�� ����

        isScrolling = false;

        // �ؽ�ó �������� �ʱ�ȭ�Ͽ� ������ ��ó�� ���̰� �մϴ�.
        if (tilemapRenderer.material != null)
        {
            tilemapRenderer.material.mainTextureOffset = Vector2.zero;
        }
        // �ð��� ��Ƽ���� ���� �ڵ带 �����߽��ϴ�.

        // 2D ���� ��Ƽ���� ���� (������)
        ApplyPhysicsMaterial(inactivePhysicsMaterial);

        // ���� ����
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.StopSFX("Running");

        Debug.Log(gameObject.name + ": �����̾� ��Ȱ��ȭ�� (���� ��Ƽ���� ����).");
    }

    // ���� ��Ƽ������ �����ϴ� ����� �Լ�
    private void ApplyPhysicsMaterial(PhysicsMaterial2D materialToApply)
    {
        if (tilemapCollider != null) // �ݶ��̴��� �ִ� ��쿡�� �õ�
        {
            if (materialToApply != null)
            {
                tilemapCollider.sharedMaterial = materialToApply;
            }
            else
            {
                Debug.LogWarning("RunningMove: �Ҵ�� Physics Material�� null�̾ ������ �� �����ϴ�.", this);
            }
        }
    }

    void OnDisable()
    {
        StopScrolling(); // ������Ʈ�� ��Ȱ��ȭ�� �� ���� ����
    }
}