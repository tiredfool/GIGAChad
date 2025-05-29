using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactStartGame : MonoBehaviour
{
    [Tooltip("GameManager ��ũ��Ʈ�� �ִ� ������Ʈ")]
    public BossManager bossManager;

    [Tooltip("�ν��� �÷��̾� �±�")]
    public string playerTag = "Player";

    private bool hasContacted = false; // �� ���� ���۵ǵ��� üũ

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasContacted && other.CompareTag(playerTag))
        {
            if (bossManager != null)
            {
                bossManager.StartAllFeatures();
                Debug.Log(gameObject.name + ": �÷��̾�� �����Ͽ� ���� ����!");
                hasContacted = true; // �� �� ���� �Ŀ��� �ٽ� ���۵��� �ʵ��� ����
                                     // (���� ����) �� Ʈ���� ������Ʈ�� �� �̻� �ʿ� ������ ��Ȱ��ȭ�ϰų� ������ �� �ֽ��ϴ�.
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false; // ��Ȱ��ȭ
                                         // Destroy(col); // ���� (����: �ٸ� ��ũ��Ʈ���� ������ �� ����)
                }
            }
            else
            {
                Debug.LogError(gameObject.name + ": GameManager�� ������� �ʾҽ��ϴ�!");
            }
        }
    }

   
}