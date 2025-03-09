using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchTrigger : MonoBehaviour
{
    private SpriteRenderer switchRenderer; // ����ġ�� ��������Ʈ ������
    private TilemapRenderer platformRenderer; // Trick ������ ������
    private TilemapCollider2D platformCollider; // Trick ������ �浹 ó��

    public Sprite greenLightSprite; // ����� �̹���
    public Sprite originalSprite; // ���� �̹��� ����
    public float activeTime = 10f; // ������ Ȱ��ȭ�Ǵ� �ð�

    void Start()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        originalSprite = switchRenderer.sprite; // ���� �̹��� ����

        // Trick �±׸� ���� ���� ã��
        GameObject platformObject = GameObject.FindGameObjectWithTag("Trick");
        if (platformObject != null)
        {
            platformRenderer = platformObject.GetComponent<TilemapRenderer>();
            platformCollider = platformObject.GetComponent<TilemapCollider2D>();

            if (platformRenderer != null) platformRenderer.enabled = false;
            if (platformCollider != null) platformCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Trick �±׸� ���� ������ ã�� �� �����ϴ�!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("����ġ �۵�: ���� Ȱ��ȭ ����");
            switchRenderer.sprite = greenLightSprite; // ����ҷ� ����
            StartCoroutine(ActivatePlatform());
        }
    }

    IEnumerator ActivatePlatform()
    {
        // ����ġ ���� ���� (0.4)
        /*
            if (switchRenderer != null)
            switchRenderer.color = new Color(1, 1, 1, 0.4f);
        */

        // ���� Ȱ��ȭ
        if (platformRenderer != null) platformRenderer.enabled = true;
        if (platformCollider != null) platformCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        // ���� ��Ȱ��ȭ
        if (platformRenderer != null) platformRenderer.enabled = false;
        if (platformCollider != null) platformCollider.enabled = false;

        // ����ġ ���� ������� (1.0)

        if (switchRenderer != null)
        {          
            switchRenderer.sprite = originalSprite; // ���� �̹����� ����
        }


        Debug.Log("���� ��Ȱ��ȭ��");
    }
}
