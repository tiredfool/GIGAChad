using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap ���� ���ӽ����̽� �߰�

public class RunningMove : MonoBehaviour
{
    [Tooltip("�ؽ�ó ��ũ�� �ӵ� (X, Y ����)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // �������� ��ũ��

    private TilemapRenderer tilemapRenderer;
    private Material materialInstance; // ��ũ���� ���� ��Ƽ���� �ν��Ͻ�
    private bool isScrolling = false; // ��ũ�� ���¸� �����ϴ� ����

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            Debug.LogError("RunningMove: TilemapRenderer�� ã�� �� �����ϴ�!", this);
            enabled = false; // ������Ʈ ��Ȱ��ȭ
            return;
        }

        // �߿�: ��Ƽ���� �ν��Ͻ��� �����Ͽ� �ٸ� Ÿ�ϸʿ� ������ ���� �ʵ��� �մϴ�.
        materialInstance = tilemapRenderer.material;
    }

    void Update()
    {
        if (isScrolling && materialInstance != null)
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

    // �ܺο��� ��ũ���� �����ϴ� �Լ�
    public void StartScrolling()
    {
        isScrolling = true;
        Debug.Log(gameObject.name + ": �ؽ�ó ��ũ�� ����");
    }

    // �ܺο��� ��ũ���� ���ߴ� �Լ�
    public void StopScrolling()
    {
        isScrolling = false;
        Debug.Log(gameObject.name + ": �ؽ�ó ��ũ�� �ߴ�");
    }

    void OnDisable()
    {
        // ��ũ���� ���� �� �������� �ʱ�ȭ�ϰų� ���ϴ� ���·� �ǵ��� �� �ֽ��ϴ�.
        if (materialInstance != null)
        {
            materialInstance.mainTextureOffset = Vector2.zero; // ����: ������ �ʱ�ȭ
        }
    }

    void OnDestroy()
    {
        // �ʿ��ϴٸ� ��Ƽ���� �ν��Ͻ��� �����մϴ�.
        // Renderer.material�� ���� �����ߴٸ� ����Ƽ�� �������� ���ɼ��� �����ϴ�.
        // ���� Start���� ��������� new Material()�� ����ߴٸ� Destroy(materialInstance); �� ȣ���ؾ� �մϴ�.
    }
}