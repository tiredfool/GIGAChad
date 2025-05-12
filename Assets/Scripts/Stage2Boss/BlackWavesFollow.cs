using UnityEngine;

public class BlackWavesFollow : MonoBehaviour
{
    public Transform targetCamera;   // 따라갈 카메라
    public float offsetY = -7f;      // 카메라보다 얼마나 아래에 위치할지

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
