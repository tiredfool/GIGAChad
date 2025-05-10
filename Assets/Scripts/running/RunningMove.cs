using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap 관련 네임스페이스 추가

public class RunningMove : MonoBehaviour
{
    [Tooltip("텍스처 스크롤 속도 (X, Y 방향)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // 왼쪽으로 스크롤

    private TilemapRenderer tilemapRenderer;
    private Material materialInstance; // 스크롤을 위한 머티리얼 인스턴스
    private bool isScrolling = false; // 스크롤 상태를 관리하는 변수

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            Debug.LogError("RunningMove: TilemapRenderer를 찾을 수 없습니다!", this);
            enabled = false; // 컴포넌트 비활성화
            return;
        }

        // 중요: 머티리얼 인스턴스를 생성하여 다른 타일맵에 영향을 주지 않도록 합니다.
        materialInstance = tilemapRenderer.material;
    }

    void Update()
    {
        if (isScrolling && materialInstance != null)
        {
            // 현재 텍스처 오프셋을 가져와서 시간에 따라 변경
            Vector2 currentOffset = materialInstance.mainTextureOffset;
            currentOffset += scrollSpeed * Time.deltaTime;

            // 오프셋 값이 너무 커지는 것을 방지 (선택 사항이지만 권장)
            currentOffset.x = currentOffset.x % 1.0f;
            currentOffset.y = currentOffset.y % 1.0f;

            // 변경된 오프셋을 머티리얼에 다시 적용
            materialInstance.mainTextureOffset = currentOffset;
        }
    }

    // 외부에서 스크롤을 시작하는 함수
    public void StartScrolling()
    {
        isScrolling = true;
        Debug.Log(gameObject.name + ": 텍스처 스크롤 시작");
    }

    // 외부에서 스크롤을 멈추는 함수
    public void StopScrolling()
    {
        isScrolling = false;
        Debug.Log(gameObject.name + ": 텍스처 스크롤 중단");
    }

    void OnDisable()
    {
        // 스크롤이 멈출 때 오프셋을 초기화하거나 원하는 상태로 되돌릴 수 있습니다.
        if (materialInstance != null)
        {
            materialInstance.mainTextureOffset = Vector2.zero; // 예시: 오프셋 초기화
        }
    }

    void OnDestroy()
    {
        // 필요하다면 머티리얼 인스턴스를 정리합니다.
        // Renderer.material을 통해 접근했다면 유니티가 관리해줄 가능성이 높습니다.
        // 만약 Start에서 명시적으로 new Material()을 사용했다면 Destroy(materialInstance); 를 호출해야 합니다.
    }
}