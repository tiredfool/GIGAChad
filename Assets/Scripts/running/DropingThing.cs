using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DropingThing : MonoBehaviour
{

    private Rigidbody2D rb;
    private ConveyorBeltPhysics currentConveyorScript = null;
    private bool isOnConveyor = false;
    private bool isGrounded = false; // 땅에 접촉했는지 여부를 저장하는 변수
    private float groundContactTime; // 땅에 처음 접촉한 시간
    public float destroyDelay = 2f;
    public float pushForce = 10f; 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("DropingThing 오브젝트에 Rigidbody 2D 컴포넌트가 없습니다!", gameObject);
        }
    }

    void FixedUpdate()
    {
        if (isOnConveyor && currentConveyorScript != null && rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            Vector2 pushDirection = currentConveyorScript.transform.right * currentConveyorScript.directionMultiplier;
            rb.AddForce(pushDirection * currentConveyorScript.pushForce, ForceMode2D.Force);
        }
        if (isGrounded && Time.time >= groundContactTime + destroyDelay)
        {
          
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 땅과의 충돌 처리 (기존 코드 유지)
        if (collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("MovingWalk"))
        {
            isGrounded = true; // 땅에 접촉했음을 기록
            groundContactTime = Time.time; // 처음 접촉한 시간
        }

        // 플레이어와의 충돌 처리 추가
        GameObject otherObject = collision.gameObject; // 충돌한 상대방의 GameObject 가져오기
        PlayerController player = otherObject.GetComponent<PlayerController>(); // GameObject에서 PlayerController 컴포넌트 가져오기

        if (player != null)
        {
            Rigidbody2D playerRb = collision.rigidbody;
            if (playerRb != null)
            {
                Vector2 pushDirection = Vector2.left; // 항상 왼쪽으로 밀도록 방향 설정
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                Debug.Log(gameObject.name + "이(가) 플레이어와 충돌하여 밀었습니다!");
                
            }
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("MovingWalk"))
        {
          
            isOnConveyor = true;
            currentConveyorScript = collision.GetComponent<ConveyorBeltPhysics>();
        }
          
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MovingWalk"))
        {
            ConveyorBeltPhysics exitedConveyor = collision.GetComponent<ConveyorBeltPhysics>();
            if (currentConveyorScript == exitedConveyor)
            {
                isOnConveyor = false;
                currentConveyorScript = null;
               // Debug.Log("DropingThing Exited Conveyor");
            }
        }
    }
}
