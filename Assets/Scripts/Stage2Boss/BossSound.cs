using UnityEngine;

public class BossSound : MonoBehaviour
{
    // 애니메이션 이벤트에서 호출할 함수
    public void PlayRippingPaperSound()
    {
        if (MainSoundManager.instance != null)
        {
            MainSoundManager.instance.PlaySFX("Ripping_Paper");
            Debug.Log("Ripping_Paper 사운드 재생");
        }
    }
}
