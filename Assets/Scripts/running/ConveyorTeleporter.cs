using UnityEngine;
using Cinemachine;
using System.Collections;

public class PlayerTeleporter : MonoBehaviour
{
    public Transform destination;
    public float ignoreCollisionDuration = 0.2f;
    public float resetDampingDelay = 0.1f;
    private Vector3 originalDamping;
    private CinemachineVirtualCamera vcam;
    private Collider2D myCollider;
    private bool first = false;
    public PlayerController P;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        vcam = FindObjectOfType<CinemachineVirtualCamera>(); // 씬에 Virtual Camera가 하나라고 가정
        if (vcam != null && vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
        {
            CinemachineTransposer transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
            originalDamping = new Vector3(transposer.m_XDamping, transposer.m_YDamping, transposer.m_ZDamping);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player") && destination != null && vcam != null)
        {
            // 텔레포트 직전 댐핑 0으로 설정
            if (vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
            {
                CinemachineTransposer transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
                transposer.m_XDamping = 0f;
                transposer.m_YDamping = 0f;
                transposer.m_ZDamping = 0f;
            }

            // 텔레포트 실행
            other.transform.position = destination.position;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if(first)P.TakeDamage();
                rb.velocity = Vector2.zero;
                first = true;
            }
            Debug.Log(other.gameObject.name + " teleported to " + destination.name);

            StartCoroutine(ResetDamping());
            StartCoroutine(IgnoreCollision(other.GetComponent<Collider2D>()));
        }
    }

    IEnumerator ResetDamping()
    {
        yield return new WaitForSeconds(resetDampingDelay);
        if (vcam != null && vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineTransposer)
        {
            CinemachineTransposer transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
            transposer.m_XDamping = originalDamping.x;
            transposer.m_YDamping = originalDamping.y;
            transposer.m_ZDamping = originalDamping.z;
        }
    }

    IEnumerator IgnoreCollision(Collider2D otherCollider)
    {
        if (myCollider != null && otherCollider != null)
        {
            Physics2D.IgnoreCollision(myCollider, otherCollider, true);
            yield return new WaitForSeconds(ignoreCollisionDuration);
            Physics2D.IgnoreCollision(myCollider, otherCollider, false);
        }
    }
}