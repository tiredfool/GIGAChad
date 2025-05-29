using UnityEngine;
using UnityEngine.Tilemaps;

public class RunningMove : MonoBehaviour
{
    [Tooltip("텍스처 스크롤 속도 (X, Y 방향)")]
    public Vector2 scrollSpeed = new Vector2(-0.5f, 0f); // 왼쪽으로 스크롤

     

    [Header("Physics Materials for Conveyor")] // 2D 물리 머티리얼을 위한 헤더
    [Tooltip("컨베이어 벨트가 활성화(움직임) 상태일 때 적용할 Physics Material 2D (마찰 낮음)")]
    public PhysicsMaterial2D activePhysicsMaterial;
    [Tooltip("컨베이어 벨트가 비활성화(멈춤) 상태일 때 적용할 Physics Material 2D (마찰 높음)")]
    public PhysicsMaterial2D inactivePhysicsMaterial;

    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider; // TilemapCollider2D 참조
    private bool isScrolling = false; // 스크롤 상태를 관리하는 변수

    void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
           // Debug.LogError("RunningMove: TilemapRenderer를 찾을 수 없습니다! 이 스크립트는 TilemapRenderer가 있는 오브젝트에 부착되어야 합니다.", this);
            enabled = false; // 컴포넌트 비활성화
            return;
        }

       

        tilemapCollider = GetComponent<TilemapCollider2D>(); // TilemapCollider2D 참조
        if (tilemapCollider == null)
        {
          //  Debug.LogError("RunningMove: TilemapCollider2D를 찾을 수 없습니다! 물리 머티리얼을 변경할 수 없습니다.", this);
        
        }

        // 스크립트 시작 시 물리 머티리얼만 비활성 상태로 설정
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
        if (isScrolling) return; // 이미 스크롤 중이면 중복 호출 방지

        isScrolling = true;

        ApplyPhysicsMaterial(activePhysicsMaterial);

        // 사운드 재생
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.PlaySFX("Running");

        Debug.Log(gameObject.name + ": 컨베이어 활성화됨 (물리 머티리얼 변경).");
    }


    public void StopScrolling()
    {
        if (!isScrolling) return; // 이미 멈춰있으면 중복 호출 방지

        isScrolling = false;

        // 텍스처 오프셋을 초기화하여 정지된 것처럼 보이게 합니다.
        if (tilemapRenderer.material != null)
        {
            tilemapRenderer.material.mainTextureOffset = Vector2.zero;
        }
        // 시각적 머티리얼 변경 코드를 제거했습니다.

        // 2D 물리 머티리얼 변경 (물리적)
        ApplyPhysicsMaterial(inactivePhysicsMaterial);

        // 사운드 중지
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.StopSFX("Running");

        Debug.Log(gameObject.name + ": 컨베이어 비활성화됨 (물리 머티리얼 복구).");
    }

    // 물리 머티리얼을 적용하는 도우미 함수
    private void ApplyPhysicsMaterial(PhysicsMaterial2D materialToApply)
    {
        if (tilemapCollider != null) // 콜라이더가 있는 경우에만 시도
        {
            if (materialToApply != null)
            {
                tilemapCollider.sharedMaterial = materialToApply;
            }
            else
            {
                Debug.LogWarning("RunningMove: 할당된 Physics Material이 null이어서 변경할 수 없습니다.", this);
            }
        }
    }

    void OnDisable()
    {
        StopScrolling(); // 오브젝트가 비활성화될 때 상태 정리
    }
}