using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap 관련 네임스페이스 추가

public class RunningMove : MonoBehaviour
{
    [Tooltip("텍스처 스크롤 속도 (X, Y 방향)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // 왼쪽으로 스크롤

    private TilemapRenderer tilemapRenderer;
    private Material originalMaterial; // 원본 머티리얼 참조 (선택적)
    private Material materialInstance; // 스크롤을 위한 머티리얼 인스턴스

    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            Debug.LogError("TilemapScroller: TilemapRenderer를 찾을 수 없습니다!", this);
            enabled = false; // 컴포넌트 비활성화
            return;
        }

        // 중요: 머티리얼 인스턴스를 생성하여 다른 타일맵에 영향을 주지 않도록 합니다.
        // Renderer.material은 자동으로 인스턴스를 생성하거나 기존 인스턴스를 반환합니다.
        materialInstance = tilemapRenderer.material;
        originalMaterial = materialInstance; // 필요시 원본 저장 (여기서는 사용 안 함)

        // 만약 여러 타일맵이 같은 머티리얼을 공유하면서 스크롤만 다르게 하고 싶다면,
        // 시작 시점에 materialInstance = new Material(tilemapRenderer.material); 로
        // 명시적으로 복제본을 만들어야 할 수도 있습니다.
        // 하지만 일반적으로 renderer.material 접근만으로 충분합니다.
    }

    void Update()
    {
        if (materialInstance != null)
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

    // 게임 오브젝트가 비활성화되거나 파괴될 때 원래 머티리얼로 돌려놓거나 인스턴스 정리 (선택적)
    void OnDisable()
    {
        // 만약 Start에서 명시적으로 new Material()을 사용했다면 메모리 누수 방지를 위해
        // if (materialInstance != null && materialInstance != originalMaterial)
        // {
        //     Destroy(materialInstance);
        //     tilemapRenderer.material = originalMaterial; // 원본으로 복구 (필요하다면)
        // }
        // Renderer.material 접근만 사용했다면 유니티가 어느 정도 관리해주지만,
        // 안전하게 하려면 아래처럼 오프셋을 초기화할 수 있습니다.
        if (materialInstance != null)
        {
            materialInstance.mainTextureOffset = Vector2.zero;
        }
    }

    // OnDestroy에서도 마찬가지로 정리해주는 것이 좋습니다.
    void OnDestroy()
    {
        // OnDisable과 유사한 정리 로직 (특히 new Material 사용 시)
        if (Application.isPlaying && materialInstance != null && materialInstance != originalMaterial)
        {
            // 게임 실행 중에만 인스턴스 파괴
            Destroy(materialInstance);
        }
    }
}