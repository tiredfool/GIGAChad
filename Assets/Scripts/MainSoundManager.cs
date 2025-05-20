using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManagement ���ӽ����̽� �߰�

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager instance;

    [System.Serializable]
    public class SFXSound
    {
        public string sfxName;
        public AudioClip sfxClip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    public List<SFXSound> sfxSoundsList;

    private Dictionary<string, SFXSound> sfxSoundDictionary = new Dictionary<string, SFXSound>();
    private Dictionary<string, AudioSource> activeSfxAudioSources = new Dictionary<string, AudioSource>();

    public AudioMixer Mixer;

    
    private Slider _bgmSlider; // �ڵ����� �Ҵ�
    private Slider _sfxSlider;

    private const float MIN_VOLUME_DB = -80f; // AudioMixer�� ���� ���� ����
    private const float MAX_VOLUME_DB = 0f;   // AudioMixer�� �⺻ ���� (����� Ŭ���� ���� ����)

    //�ʱ⺼��
    private float currentBGMVolume = 1f; 
    private float currentSFXVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (SFXSound sfx in sfxSoundsList)
        {
            if (sfxSoundDictionary.ContainsKey(sfx.sfxName))
            {
                Debug.LogWarning("�ߺ��� SFX �̸�: " + sfx.sfxName + ". ù ��° �׸� ���˴ϴ�.");
                continue;
            }
            sfxSoundDictionary.Add(sfx.sfxName, sfx);
        }

        ApplyInitialMixerVolumes();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"�� '{scene.name}' �ε� �Ϸ�. UI �����̴��� �ٽ� �����ϰ� ���� ������ �����մϴ�.");
        ConnectSlidersAndApplyVolumes(); // �����̴� ���� �õ� �� ���� ����
    }

    // �ʱ� Awake ������ �ͼ� ������ �����ϴ� �Լ�
    private void ApplyInitialMixerVolumes()
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer�� MainSoundManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // BGM ���� ����
        float bgmDb = (currentBGMVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentBGMVolume) * 20;
        Mixer.SetFloat("BGM", bgmDb);
        Debug.Log($"�ʱ� BGM ���� ���� (Mixer Only): {currentBGMVolume}, dB: {bgmDb}");

        // SFX ���� ����
        float sfxDb = (currentSFXVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentSFXVolume) * 20;
        Mixer.SetFloat("SFX", sfxDb);
        Debug.Log($"�ʱ� SFX ���� ���� (Mixer Only): {currentSFXVolume}, dB: {sfxDb}");
    }


    // �����̴��� ã�� �����ϰ�, ���� ������ �����ϴ� �Լ�
    private void ConnectSlidersAndApplyVolumes()
    {
        // ������ ��� Slider ������Ʈ�� ã���ϴ�.
        Slider[] allSliders = FindObjectsOfType<Slider>(true);
        foreach (Slider s in allSliders)
        {
            if (s.name == "BGMSlider") // �����̴� UI ������Ʈ�� �̸��� "BGMSlider"��� ����
            {
                _bgmSlider = s;
            }
            else if (s.name == "SFXSlider") // �����̴� UI ������Ʈ�� �̸��� "SFXSlider"��� ����
            {
                _sfxSlider = s;
            }
        }

        // --- BGM �����̴� ���� �� ���� ���� ���� ---
        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveAllListeners(); // ���� ������ ���� (�ߺ� ���� ����)
            _bgmSlider.onValueChanged.AddListener(SetBGMVolume); // �� ������ ���� (�Ű����� �ִ� ����)

            // �����̴� UI�� ���� ���� ����� ���� ����
            _bgmSlider.value = currentBGMVolume;
            SetBGMVolume(currentBGMVolume);
            Debug.Log($"BGM �����̴��� ���� �� ���� �� �ͼ� ������Ʈ: {currentBGMVolume}");
        }
        else
        {
            Debug.LogWarning("BGMSlider�� ������ ã�� �� �����ϴ�! �̸��� 'BGMSlider'���� Ȯ���ϼ���.");
           
            float bgmDb = (currentBGMVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentBGMVolume) * 20;
            Mixer.SetFloat("BGM", bgmDb);
        }

      
        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);

            _sfxSlider.value = currentSFXVolume;
            SetSFXVolume(currentSFXVolume);
            Debug.Log($"SFX �����̴��� ���� �� ���� �� �ͼ� ������Ʈ: {currentSFXVolume}");
        }
        else
        {
            Debug.LogWarning("SFXSlider�� ������ ã�� �� �����ϴ�! �̸��� 'SFXSlider'���� Ȯ���ϼ���.");
            float sfxDb = (currentSFXVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentSFXVolume) * 20;
            Mixer.SetFloat("SFX", sfxDb);
        }
    }

    // �����̴� ���� ����� ������ ȣ��Ǹ�, �� ���� currentBGMVolume�� ����˴ϴ�.
    public void SetBGMVolume(float sliderValue)
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer�� MainSoundManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        float volumeDb;
        if (sliderValue <= 0.0001f)
        {
            volumeDb = MIN_VOLUME_DB;
        }
        else
        {
            volumeDb = Mathf.Log10(sliderValue) * 20;
        }
        Mixer.SetFloat("BGM", volumeDb);
        currentBGMVolume = sliderValue; // ����� �����̴� ���� ���� ������ ����
        Debug.Log($"BGM Slider Value changed and stored: {sliderValue}, BGM Volume dB: {volumeDb}");
    }

    // �����̴� ���� ����� ������ ȣ��Ǹ�, �� ���� currentSFXVolume�� ����˴ϴ�.
    public void SetSFXVolume(float sliderValue)
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer�� MainSoundManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        float volumeDb;
        if (sliderValue <= 0.0001f)
        {
            volumeDb = MIN_VOLUME_DB;
        }
        else
        {
            volumeDb = Mathf.Log10(sliderValue) * 20;
        }
        Mixer.SetFloat("SFX", volumeDb);
        currentSFXVolume = sliderValue; // ����� �����̴� ���� ���� ������ ����
        Debug.Log($"SFX Slider Value changed and stored: {sliderValue}, SFX Volume dB: {volumeDb}");
    }

    public void ToggleAudioVolume()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    }

    public void PlaySFX(string sfxName)
    {
        if (sfxSoundDictionary.TryGetValue(sfxName, out SFXSound sfx))
        {
            AudioSource targetSource;

            if (sfx.loop)
            {
                if (activeSfxAudioSources.TryGetValue(sfxName, out targetSource) && targetSource != null && targetSource.isPlaying)
                {
                    return;
                }
            }

            if (!activeSfxAudioSources.TryGetValue(sfxName, out targetSource) || targetSource == null)
            {
                targetSource = gameObject.AddComponent<AudioSource>();
                if (Mixer != null)
                {
                    AudioMixerGroup[] sfxGroups = Mixer.FindMatchingGroups("SFX");
                    if (sfxGroups.Length > 0)
                    {
                        targetSource.outputAudioMixerGroup = sfxGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning("AudioMixer���� 'SFX' �׷��� ã�� �� �����ϴ�. SFX AudioSource�� �ͼ��� ������� �ʽ��ϴ�.");
                    }
                }
                else
                {
                    Debug.LogWarning("MainSoundManager�� Mixer�� �Ҵ���� �ʾ� SFX AudioSource�� �ͼ��� ������ �� �����ϴ�.");
                }
                targetSource.playOnAwake = false;
                activeSfxAudioSources[sfxName] = targetSource;
            }

            targetSource.clip = sfx.sfxClip;
            targetSource.volume = sfx.volume;
            targetSource.loop = sfx.loop;

            if (sfx.loop)
            {
                targetSource.Play();
            }
            else
            {
                targetSource.PlayOneShot(sfx.sfxClip);
                StartCoroutine(CleanUpAudioSource(targetSource, sfx.sfxClip.length));
            }
        }
        else
        {
            Debug.LogWarning("SFX '" + sfxName + "'�� ã�� �� �����ϴ�. MainSoundManager�� sfxSoundsList�� Ȯ���ϼ���.");
        }
    }

    public void StopSFX(string sfxName)
    {
        if (activeSfxAudioSources.TryGetValue(sfxName, out AudioSource targetSource))
        {
            if (targetSource != null && targetSource.isPlaying)
            {
                targetSource.Stop();
                if (sfxSoundDictionary.TryGetValue(sfxName, out SFXSound sfx) && !sfx.loop)
                {
                    activeSfxAudioSources.Remove(sfxName);
                    Destroy(targetSource);
                }
            }
        }
    }

    private IEnumerator CleanUpAudioSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            string sfxNameFound = "";
            foreach (var entry in activeSfxAudioSources)
            {
                if (entry.Value == source)
                {
                    sfxNameFound = entry.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(sfxNameFound))
            {
                activeSfxAudioSources.Remove(sfxNameFound);
            }
            Destroy(source);
        }
    }
}