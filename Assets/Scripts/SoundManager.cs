using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public enum EAudioMixerType { Master, BGM, SFX }
public class SoundManager : MonoBehaviour
{
    public AudioMixer Mixer;
    public Slider audioSlider;

    public void AduioControl()
    {
        float sound = audioSlider.value;

        if (sound == -40f) Mixer.SetFloat("BGM", -80);
        else Mixer.SetFloat("BGM",sound);
    }

    public void ToggleAudioVolume()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    }
}


