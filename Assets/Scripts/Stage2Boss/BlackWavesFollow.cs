using UnityEngine;

public class BlackWavesFollow : MonoBehaviour
{
    public Transform targetCamera;   // ���� ī�޶�
    public float offsetY = -7f;      // ī�޶󺸴� �󸶳� �Ʒ��� ��ġ����

    void Update()
    {
        if (targetCamera != null)
        {
            Vector3 newPos = transform.position;
            newPos.y = targetCamera.position.y + offsetY;
            transform.position = newPos;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Stage2Manager.instance.EndGameByBlackWave();
        }
    }

}
