using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    SpriteRenderer sr;
    Animator anim;
    public int hp;

    Rigidbody2D rb;

    public BossManager bossManager;

    private bool hasReducedDuration = false;

    private VirtualInputManager virtualInputManager;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr= GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        hp = 3;

        if (bossManager == null)
        {
            bossManager = FindObjectOfType<BossManager>();
        }
        virtualInputManager = VirtualInputManager.Instance;
        if (virtualInputManager == null)
        {
            Debug.LogError("VirtualInputManager �ν��Ͻ��� ã�� �� �����ϴ�. ���� VirtualInputManager�� �ִ��� Ȯ�����ּ���.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualInputManager != null)
        {
            // WASD�� ���� ��ư���� Ȱ���Ͽ� inputVec ����
            float horizontal = 0;
            float vertical = 0;

            if (virtualInputManager.GetKeyOrButton("Left"))
            {
                horizontal -= 1;
            }
            if (virtualInputManager.GetKeyOrButton("Right"))
            {
                horizontal += 1;
            }
            if (virtualInputManager.GetKeyOrButton("Up"))
            {
                vertical += 1;
            }
            if (virtualInputManager.GetKeyOrButton("Down"))
            {
                vertical -= 1;
            }

            inputVec = new Vector2(horizontal, vertical);
        }
        else
        {
            // VirtualInputManager�� ���� ��� ���� Input.GetAxisRaw ��� ���� (������ �Ǵ� Fallback)
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
        }
    }
    private void FixedUpdate()
    {
        //1.���� �ش�
        //rb.AddForce(inputVec);
        //2.�ӵ� ����
        //rb.velocity = inputVec;
        //3.��ġ�̵�
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
    }

    void LateUpdate()
    {
        anim.SetFloat("Speed", inputVec.magnitude);//������ ����
        if (inputVec.x != 0)
        {
            sr.flipX = inputVec.x <0;
        }
    }

    public void EndMiniGame()
    {
        if (hp> 0)
        {
            bossManager.ReduceActiveDuration(30f);
        }
    }
}
