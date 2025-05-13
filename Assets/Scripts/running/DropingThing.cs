using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DropingThing : MonoBehaviour
{

    private Rigidbody2D rb;
    private ConveyorBeltPhysics currentConveyorScript = null;
    private bool isOnConveyor = false;
    private bool isGrounded = false; // ���� �����ߴ��� ���θ� �����ϴ� ����
    private float groundContactTime; // ���� ó�� ������ �ð�
    public float destroyDelay = 2f;
    public float pushForce = 10f; 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("DropingThing ������Ʈ�� Rigidbody 2D ������Ʈ�� �����ϴ�!", gameObject);
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
        // ������ �浹 ó�� (���� �ڵ� ����)
        if (collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("MovingWalk"))
        {
            isGrounded = true; // ���� ���������� ���
            groundContactTime = Time.time; // ó�� ������ �ð�
        }

        // �÷��̾���� �浹 ó�� �߰�
        GameObject otherObject = collision.gameObject; // �浹�� ������ GameObject ��������
        PlayerController player = otherObject.GetComponent<PlayerController>(); // GameObject���� PlayerController ������Ʈ ��������

        if (player != null)
        {
            Rigidbody2D playerRb = collision.rigidbody;
            if (playerRb != null)
            {
                Vector2 pushDirection = Vector2.left; // �׻� �������� �е��� ���� ����
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                Debug.Log(gameObject.name + "��(��) �÷��̾�� �浹�Ͽ� �о����ϴ�!");
                
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
