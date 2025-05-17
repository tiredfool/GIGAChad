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

    public void AduioControl()
    {
        float bgmsound = BGMSlider.value;
        float sfxsound = SFXSlider.value;

        if (bgmsound == -40f) Mixer.SetFloat("BGM", -80);
        else Mixer.SetFloat("BGM", bgmsound);

        if (sfxsound == -40f) Mixer.SetFloat("SFX", -80);
        else Mixer.SetFloat("SFX", sfxsound);
    }

    public void ToggleAudioVolume()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    }
}


