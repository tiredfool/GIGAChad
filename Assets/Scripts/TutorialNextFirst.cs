using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNextFirst : MonoBehaviour
{
    private BoxCollider2D col;
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (GameManager.instance != null)
        {
            int hopeScore = GameManager.instance.hopeScore;

            if (hopeScore >= 1)
            {
                col.isTrigger = true;  // ������ 5�� �̻��̸� ��� ����
            }
            else
            {
                col.isTrigger = false; // ������ �����ϸ� ��ó�� ����
            }
        }
    }
}
