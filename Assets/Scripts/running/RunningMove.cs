using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap ���� ���ӽ����̽� �߰�

public class RunningMove : MonoBehaviour
{
    [Tooltip("�ؽ�ó ��ũ�� �ӵ� (X, Y ����)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // �������� ��ũ��

    private TilemapRenderer tilemapRenderer;
    private Material originalMaterial; // ���� ��Ƽ���� ���� (������)
    private Material materialInstance; // ��ũ���� ���� ��Ƽ���� �ν��Ͻ�

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            Debug.LogError("TilemapScroller: TilemapRenderer�� ã�� �� �����ϴ�!", this);
            enabled = false; // ������Ʈ ��Ȱ��ȭ
            return;
        }

        // �߿�: ��Ƽ���� �ν��Ͻ��� �����Ͽ� �ٸ� Ÿ�ϸʿ� ������ ���� �ʵ��� �մϴ�.
        // Renderer.material�� �ڵ����� �ν��Ͻ��� �����ϰų� ���� �ν��Ͻ��� ��ȯ�մϴ�.
        materialInstance = tilemapRenderer.material;
        originalMaterial = materialInstance; // �ʿ�� ���� ���� (���⼭�� ��� �� ��)

        // ���� ���� Ÿ�ϸ��� ���� ��Ƽ������ �����ϸ鼭 ��ũ�Ѹ� �ٸ��� �ϰ� �ʹٸ�,
        // ���� ������ materialInstance = new Material(tilemapRenderer.material); ��
        // ��������� �������� ������ �� ���� �ֽ��ϴ�.
        // ������ �Ϲ������� renderer.material ���ٸ����� ����մϴ�.
    }

    void Update()
    {
        if (materialInstance != null)
        {
            // ���� �ؽ�ó �������� �����ͼ� �ð��� ���� ����
            Vector2 currentOffset = materialInstance.mainTextureOffset;
            currentOffset += scrollSpeed * Time.deltaTime;

            // ������ ���� �ʹ� Ŀ���� ���� ���� (���� ���������� ����)
            currentOffset.x = currentOffset.x % 1.0f;
            currentOffset.y = currentOffset.y % 1.0f;

            // ����� �������� ��Ƽ���� �ٽ� ����
            materialInstance.mainTextureOffset = currentOffset;
        }
    }

    // ���� ������Ʈ�� ��Ȱ��ȭ�ǰų� �ı��� �� ���� ��Ƽ����� �������ų� �ν��Ͻ� ���� (������)
    void OnDisable()
    {
        // ���� Start���� ��������� new Material()�� ����ߴٸ� �޸� ���� ������ ����
        // if (materialInstance != null && materialInstance != originalMaterial)
        // {
        //     Destroy(materialInstance);
        //     tilemapRenderer.material = originalMaterial; // �������� ���� (�ʿ��ϴٸ�)
        // }
        // Renderer.material ���ٸ� ����ߴٸ� ����Ƽ�� ��� ���� ������������,
        // �����ϰ� �Ϸ��� �Ʒ�ó�� �������� �ʱ�ȭ�� �� �ֽ��ϴ�.
        if (materialInstance != null)
        {
            materialInstance.mainTextureOffset = Vector2.zero;
        }
    }

    // OnDestroy������ ���������� �������ִ� ���� �����ϴ�.
    void OnDestroy()
    {
        // OnDisable�� ������ ���� ���� (Ư�� new Material ��� ��)
        if (Application.isPlaying && materialInstance != null && materialInstance != originalMaterial)
        {
            // ���� ���� �߿��� �ν��Ͻ� �ı�
            Destroy(materialInstance);
        }
    }
}