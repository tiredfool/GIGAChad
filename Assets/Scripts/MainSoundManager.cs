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
    // �����̴� �ʵ带 public���� �����ϵ�, Inspector���� ���� �Ҵ����� �ʾƵ� �ǵ��� �մϴ�.
    // public Slider BGMSlider; // ���� �ڵ� ������ ���̹Ƿ� Inspector���� �����ص� �˴ϴ�.
    // public Slider SFXSlider; // ���� �ڵ� ������ ���̹Ƿ� Inspector���� �����ص� �˴ϴ�.

    // private ������ �����ϰ� �ʿ��� �� ã�� �Ҵ��մϴ�.
    private Slider _bgmSlider;
    private Slider _sfxSlider;

    private const float MIN_VOLUME_DB = -80f;
    private const float MAX_VOLUME_DB = 0f;

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
    }

    void OnEnable()
    {
        // �� �ε� �̺�Ʈ ����: ���ο� ���� �ε�� ������ OnSceneLoaded �Լ��� ȣ��˴ϴ�.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // ������Ʈ�� ��Ȱ��ȭ�ǰų� �ı��� �� �̺�Ʈ ���� ���� (�޸� ���� ����)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �� �ε� �Ϸ� �� ȣ��� �Լ�
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"�� '{scene.name}' �ε� �Ϸ�. UI �����̴��� �ٽ� �����մϴ�.");
        ConnectSliders(); // �����̴� ���� �õ�
    }

    // �����̴��� ã�� �����ϰ�, �ʱ� ������ �����ϴ� �Լ�
    private void ConnectSliders()
    {
        // ������ "BGMSlider"��� �̸��� Slider ������Ʈ�� ã���ϴ�. (�̸��� ���� UI ������Ʈ �̸��� ��ġ�ؾ� �մϴ�)
        _bgmSlider = FindObjectOfType<Slider>(); // Ư�� �̸����� ã������ GameObject.Find("BGMSlider").GetComponent<Slider>();
                                                 // �� ����� ���� �����̴��� �ϳ��� �ְų�, Ư�� �±� ������ ������ �� ���� �� �����մϴ�.
                                                 // ���� ���, UI ĵ���� ���� �����̴��� �ִٸ� �� ��ü���� ��θ� ����� �� �ֽ��ϴ�:
                                                 // Transform canvasTransform = GameObject.Find("Canvas").transform;
                                                 // _bgmSlider = canvasTransform.Find("BGMSlider").GetComponent<Slider>();


        // SFX �����̴� ã��
        // FindObjectsOfType<Slider>()�� ����Ͽ� ��� �����̴��� ã�� ��,
        // �̸����� �����ϴ� ���� �Ϲ����Դϴ�.
        Slider[] allSliders = FindObjectsOfType<Slider>(true); // ��Ȱ��ȭ�� ������Ʈ�� ã�� ���� true �߰�
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


        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveAllListeners(); // ��� ������ ����
            _bgmSlider.onValueChanged.AddListener(SetBGMVolume); // BGM ���� �Լ� ����
        }
        else
        {
            Debug.LogWarning("BGMSlider�� ������ ã�� �� �����ϴ�! �̸��� 'BGMSlider'���� Ȯ���ϼ���.");
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveAllListeners(); // ��� ������ ����
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume); // SFX ���� �Լ� ����
        }
        else
        {
            Debug.LogWarning("SFXSlider�� ������ ã�� �� �����ϴ�! �̸��� 'SFXSlider'���� Ȯ���ϼ���.");
        }

        if (_bgmSlider != null) SetBGMVolume(_bgmSlider.value);
        if (_sfxSlider != null) SetSFXVolume(_sfxSlider.value);
    }

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
        Debug.Log($"BGM Slider Value: {sliderValue}, BGM Volume dB: {volumeDb}");
    }

    // ����� �κ�: �����̴� ���� �Ű������� ���� �޽��ϴ�.
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
        Debug.Log($"SFX Slider Value: {sliderValue}, SFX Volume dB: {volumeDb}");
    }
    // �� �Լ��� ���� ���������� _bgmSlider�� _sfxSlider�� �����մϴ�.
    public void AduioControl()
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer�� MainSoundManager�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
        // ���� �ʵ带 ������� �ʰ�, private ������ ����մϴ�.
        if (_bgmSlider == null)
        {
            // Debug.LogError("BGMSlider�� ���� ������� �ʾҽ��ϴ�!"); // ������ �����߰ų� ���� UI�� �ε���� ���� ��� �߻�
            return;
        }
        if (_sfxSlider == null)
        {
            // Debug.LogError("SFXSlider�� ���� ������� �ʾҽ��ϴ�!"); // ������ �����߰ų� ���� UI�� �ε���� ���� ��� �߻�
            return;
        }

        // BGM ���� ����
        float bgmSliderValue = _bgmSlider.value; // _bgmSlider ���
        float bgmVolumeDb;
        if (bgmSliderValue <= 0.0001f)
        {
            bgmVolumeDb = MIN_VOLUME_DB;
        }
        else
        {
            bgmVolumeDb = Mathf.Log10(bgmSliderValue) * 20;
        }
        Mixer.SetFloat("BGM", bgmVolumeDb);
        Debug.Log($"BGM Slider Value: {bgmSliderValue}, BGM Volume dB: {bgmVolumeDb}");

        // SFX ���� ����
        float sfxSliderValue = _sfxSlider.value; // _sfxSlider ���
        float sfxVolumeDb;
        if (sfxSliderValue <= 0.0001f)
        {
            sfxVolumeDb = MIN_VOLUME_DB;
        }
        else
        {
            sfxVolumeDb = Mathf.Log10(sfxSliderValue) * 20;
        }
        Mixer.SetFloat("SFX", sfxVolumeDb);
        Debug.Log($"SFX Slider Value: {sfxSliderValue}, SFX Volume dB: {sfxVolumeDb}");
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