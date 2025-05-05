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
        Debug.Log("Collision Enter with: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("MovingWalk"))
        {
            isGrounded = true; // 땅에 접촉했음을 기록
            groundContactTime = Time.time; // 처음 접촉한 시간 기록
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
                Debug.Log("DropingThing Exited Conveyor");
            }
        }
    }
}
