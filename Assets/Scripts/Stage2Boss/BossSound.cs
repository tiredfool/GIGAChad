using UnityEngine;

public class BossSound : MonoBehaviour
{
    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �Լ�
    public void PlayRippingPaperSound()
    {
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.PlaySFX("Ripping_Paper");
            Debug.Log("Ripping_Paper ���� ���");
        }
    }
}
