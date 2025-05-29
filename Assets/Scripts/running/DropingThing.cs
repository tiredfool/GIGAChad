using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropingThing : MonoBehaviour
{
    private Rigidbody2D rb;
    private ConveyorBeltPhysics currentConveyorScript = null;

    private bool isGrounded = false;
    private float groundContactTime;
    public float destroyDelay = 2f;

    // 🔴 새로 추가된 변수: 트리거에 처음 닿을 때 튕겨져 나가는 힘의 크기
    public float initialKnockbackForceOnTrigger = 200f; // 이 값을 인스펙터에서 조절하여 튕겨져 나가는 정도를 튜닝하세요.

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
           // Debug.LogError("DropingThing 오브젝트에 Rigidbody 2D 컴포넌트가 없습니다!", gameObject);
        }
    }

    void FixedUpdate()
    {
        // 땅에 닿은 후 파괴되는 로직은 유지
        if (isGrounded && Time.time >= groundContactTime + destroyDelay)
        {
            if (MainSoundManager.instance != null) MainSoundManager.instance.PlaySFX("Eating");
            Destroy(gameObject);
        }
    }

    // OnCollisionEnter2D는 Ground, Player와 같은 트리거가 아닌 콜라이더와의 물리적 충돌에 사용합니다.
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground 태그는 트리거가 아니므로 여기에 유지
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            groundContactTime = Time.time;
            //Debug.Log("DropingThing: Touched Ground.");
        }

        // 플레이어와의 충돌 처리 (플레이어는 트리거가 아닌 콜라이더이므로 여기에 유지)
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.PlaySFX("Eating");
            }
           // Debug.Log("DropingThing: Touched Player.");
        }
    }

    // 🔴 OnTriggerEnter2D: MovingWalk 트리거에 처음 진입했을 때 (튕겨나가는 역할)
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("DropingThing: Triggered with " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");

        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            // 컨베이어 벨트 스크립트 참조 가져오기 (OnTriggerStay2D에서 사용할 것이므로 필요)
            currentConveyorScript = collision.GetComponent<ConveyorBeltPhysics>();
            if (currentConveyorScript == null)
            {
               // Debug.LogError("ConveyorBeltPhysics 스크립트를 MovingWalk 오브젝트에서 찾을 수 없습니다!", collision.gameObject);
            }

            // 🔴 트리거에 처음 닿을 때 왼쪽으로 강하게 튕겨내는 힘 적용
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 knockbackDirection = Vector2.left; // 왼쪽 방향

                // Impulse 모드로 강한 힘을 한 번만 가합니다.
                // initialKnockbackForceOnTrigger 값을 인스펙터에서 튜닝하세요.
                rb.AddForce(knockbackDirection * initialKnockbackForceOnTrigger, ForceMode2D.Impulse);

                //Debug.Log($"DropingThing: Entered MovingWalk Trigger. Applied initial knockback force: {initialKnockbackForceOnTrigger}");
            }
        }
    }

    // 🔴 OnTriggerStay2D: MovingWalk 트리거 안에 머무는 동안 계속 호출 (지속적인 힘)
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic && currentConveyorScript != null)
            {
                // 항상 왼쪽으로 밀려면 Vector2.left를 사용합니다.
                Vector2 pushDirection = Vector2.left;

                // ConveyorBeltPhysics 스크립트의 pushForce 값을 가져와서 사용합니다.
                // ForceMode2D.Force는 지속적인 힘을 가할 때 적합합니다.
                rb.AddForce(pushDirection * currentConveyorScript.pushForce*10f, ForceMode2D.Force);

                //Debug.Log($"DropingThing: Staying on MovingWalk. Applying continuous force: {pushDirection * currentConveyorScript.pushForce}. Current Velocity: {rb.velocity}");
            }
        }
    }

    // 🔴 OnTriggerExit2D: MovingWalk 트리거에서 벗어날 때
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            // 컨베이어 벨트 스크립트 참조 초기화
            if (currentConveyorScript == collision.GetComponent<ConveyorBeltPhysics>())
            {
                currentConveyorScript = null;
                //Debug.Log("DropingThing: Exited MovingWalk Trigger.");
            }
        }
    }
}