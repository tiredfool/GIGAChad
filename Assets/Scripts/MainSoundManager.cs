using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManagement 네임스페이스 추가

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

    
    private Slider _bgmSlider; // 자동으로 할당
    private Slider _sfxSlider;

    private const float MIN_VOLUME_DB = -80f; // AudioMixer의 가장 낮은 볼륨
    private const float MAX_VOLUME_DB = 0f;   // AudioMixer의 기본 볼륨 (오디오 클립의 원본 볼륨)

    //초기볼륨
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
                Debug.LogWarning("중복된 SFX 이름: " + sfx.sfxName + ". 첫 번째 항목만 사용됩니다.");
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
        Debug.Log($"씬 '{scene.name}' 로드 완료. UI 슬라이더를 다시 연결하고 현재 볼륨을 적용합니다.");
        ConnectSlidersAndApplyVolumes(); // 슬라이더 연결 시도 및 볼륨 적용
    }

    // 초기 Awake 시점에 믹서 볼륨을 설정하는 함수
    private void ApplyInitialMixerVolumes()
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer가 MainSoundManager에 할당되지 않았습니다!");
            return;
        }

        // BGM 볼륨 적용
        float bgmDb = (currentBGMVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentBGMVolume) * 20;
        Mixer.SetFloat("BGM", bgmDb);
        Debug.Log($"초기 BGM 볼륨 적용 (Mixer Only): {currentBGMVolume}, dB: {bgmDb}");

        // SFX 볼륨 적용
        float sfxDb = (currentSFXVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentSFXVolume) * 20;
        Mixer.SetFloat("SFX", sfxDb);
        Debug.Log($"초기 SFX 볼륨 적용 (Mixer Only): {currentSFXVolume}, dB: {sfxDb}");
    }


    // 슬라이더를 찾아 연결하고, 현재 볼륨을 설정하는 함수
    private void ConnectSlidersAndApplyVolumes()
    {
        // 씬에서 모든 Slider 컴포넌트를 찾습니다.
        Slider[] allSliders = FindObjectsOfType<Slider>(true);
        foreach (Slider s in allSliders)
        {
            if (s.name == "BGMSlider") // 슬라이더 UI 오브젝트의 이름이 "BGMSlider"라고 가정
            {
                _bgmSlider = s;
            }
            else if (s.name == "SFXSlider") // 슬라이더 UI 오브젝트의 이름이 "SFXSlider"라고 가정
            {
                _sfxSlider = s;
            }
        }

        // --- BGM 슬라이더 연결 및 현재 볼륨 적용 ---
        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveAllListeners(); // 기존 리스너 제거 (중복 연결 방지)
            _bgmSlider.onValueChanged.AddListener(SetBGMVolume); // 새 리스너 연결 (매개변수 있는 버전)

            // 슬라이더 UI의 값에 현재 저장된 볼륨 적용
            _bgmSlider.value = currentBGMVolume;
            SetBGMVolume(currentBGMVolume);
            Debug.Log($"BGM 슬라이더에 현재 값 적용 및 믹서 업데이트: {currentBGMVolume}");
        }
        else
        {
            Debug.LogWarning("BGMSlider를 씬에서 찾을 수 없습니다! 이름이 'BGMSlider'인지 확인하세요.");
           
            float bgmDb = (currentBGMVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentBGMVolume) * 20;
            Mixer.SetFloat("BGM", bgmDb);
        }

      
        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);

            _sfxSlider.value = currentSFXVolume;
            SetSFXVolume(currentSFXVolume);
            Debug.Log($"SFX 슬라이더에 현재 값 적용 및 믹서 업데이트: {currentSFXVolume}");
        }
        else
        {
            Debug.LogWarning("SFXSlider를 씬에서 찾을 수 없습니다! 이름이 'SFXSlider'인지 확인하세요.");
            float sfxDb = (currentSFXVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(currentSFXVolume) * 20;
            Mixer.SetFloat("SFX", sfxDb);
        }
    }

    // 슬라이더 값이 변경될 때마다 호출되며, 이 값이 currentBGMVolume에 저장됩니다.
    public void SetBGMVolume(float sliderValue)
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer가 MainSoundManager에 할당되지 않았습니다!");
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
        currentBGMVolume = sliderValue; // 변경된 슬라이더 값을 내부 변수에 저장
        Debug.Log($"BGM Slider Value changed and stored: {sliderValue}, BGM Volume dB: {volumeDb}");
    }

    // 슬라이더 값이 변경될 때마다 호출되며, 이 값이 currentSFXVolume에 저장됩니다.
    public void SetSFXVolume(float sliderValue)
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer가 MainSoundManager에 할당되지 않았습니다!");
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
        currentSFXVolume = sliderValue; // 변경된 슬라이더 값을 내부 변수에 저장
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
                        Debug.LogWarning("AudioMixer에서 'SFX' 그룹을 찾을 수 없습니다. SFX AudioSource가 믹서에 연결되지 않습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("MainSoundManager의 Mixer가 할당되지 않아 SFX AudioSource를 믹서에 연결할 수 없습니다.");
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
            Debug.LogWarning("SFX '" + sfxName + "'를 찾을 수 없습니다. MainSoundManager의 sfxSoundsList를 확인하세요.");
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