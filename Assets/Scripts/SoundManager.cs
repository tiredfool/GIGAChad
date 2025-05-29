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
        // ���� ���� �� �����̴��� ���� �ʱ� ���� ������ ����
        if (BGMSlider != null)
        {
            BGMSlider.value = SoundSettings.Instance.MasterBGMVolume;
        }
        if (SFXSlider != null)
        {
            SFXSlider.value = SoundSettings.Instance.MasterSFXVolume;
        }

        // �ʱ� ���� ���� �� �ٷ� �ͼ��� ����
        AudioControl();
    }
    //public void AudioControl()
    //{
    //    float bgmsound = BGMSlider.value;
    //    float sfxSliderValue = SFXSlider.value;

    //    // �����̴� ���� dB �����Ϸ� ��ȯ�Ͽ� �ͼ��� ����
    //    // �����̴� ���� 0�� ������ -80dB (���� ���Ұ�)�� ����
    //    float bgmDb = (bgmSliderValue <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(bgmSliderValue) * 20;
    //    float sfxDb = (sfxSliderValue <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(sfxSliderValue) * 20;

    //    Mixer.SetFloat("BGM", bgmDb);
    //    Mixer.SetFloat("SFX", sfxDb);

    //    Debug.Log($"BGM �����̴� ��: {bgmSliderValue}, �ͼ� BGM dB: {bgmDb}");
    //    Debug.Log($"SFX �����̴� ��: {sfxSliderValue}, �ͼ� SFX dB: {sfxDb}");
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
            bgmDb = MIN_VOLUME_DB; // ���Ұſ� �ش��ϴ� �ּ� dB �� ����
        }
        else
        {
            bgmDb = Mathf.Log10(bgmsound) * 20; 
        }

        float sfxDb;
        if (sfxsound <= 0.0001f) 
        {
            sfxDb = MIN_VOLUME_DB; // ���Ұſ� �ش��ϴ� �ּ� dB �� ����
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


