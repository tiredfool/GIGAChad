using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public enum EAudioMixerType { Master, BGM, SFX }
public class SoundManager : MonoBehaviour
{


    public AudioMixer Mixer;
    public Slider BGMSlider;
    public Slider SFXSlider;
    private const float MIN_VOLUME_DB = -80f;
    void Start()
    {
        // 게임 시작 시 슬라이더의 값을 초기 볼륨 변수로 설정
        if (BGMSlider != null)
        {
            BGMSlider.value = SoundSettings.Instance.MasterBGMVolume;
        }
        if (SFXSlider != null)
        {
            SFXSlider.value = SoundSettings.Instance.MasterSFXVolume;
        }

        // 초기 볼륨 설정 후 바로 믹서에 적용
        AudioControl();
    }
    //public void AudioControl()
    //{
    //    float bgmsound = BGMSlider.value;
    //    float sfxSliderValue = SFXSlider.value;

    //    // 슬라이더 값을 dB 스케일로 변환하여 믹서에 적용
    //    // 슬라이더 값이 0에 가까우면 -80dB (거의 음소거)로 설정
    //    float bgmDb = (bgmSliderValue <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(bgmSliderValue) * 20;
    //    float sfxDb = (sfxSliderValue <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(sfxSliderValue) * 20;

    //    Mixer.SetFloat("BGM", bgmDb);
    //    Mixer.SetFloat("SFX", sfxDb);

    //    Debug.Log($"BGM 슬라이더 값: {bgmSliderValue}, 믹서 BGM dB: {bgmDb}");
    //    Debug.Log($"SFX 슬라이더 값: {sfxSliderValue}, 믹서 SFX dB: {sfxDb}");
    //}
    public void AudioControl()
    {
        float bgmsound = BGMSlider.value;
        float sfxsound = SFXSlider.value;
        SoundSettings.Instance.MasterBGMVolume = bgmsound;
        SoundSettings.Instance.MasterSFXVolume = sfxsound;
      
        float bgmDb;
        if (bgmsound <= 0.0001f) 
        {
            bgmDb = MIN_VOLUME_DB; // 음소거에 해당하는 최소 dB 값 적용
        }
        else
        {
            bgmDb = Mathf.Log10(bgmsound) * 20; 
        }

        float sfxDb;
        if (sfxsound <= 0.0001f) 
        {
            sfxDb = MIN_VOLUME_DB; // 음소거에 해당하는 최소 dB 값 적용
        }
        else
        {
            sfxDb = Mathf.Log10(sfxsound) * 20; 
        }
        Mixer.SetFloat("BGM", bgmDb);
        Mixer.SetFloat("SFX", sfxDb);


    }

    public void ToggleAudioVolume()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    }
}


