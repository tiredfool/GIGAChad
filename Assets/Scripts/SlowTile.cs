using UnityEngine;

public class SlowTile : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // �̵� �ӵ��� ���� ����

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplySlow(slowMultiplier);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RemoveSlow();
            }
        }
    }
}
