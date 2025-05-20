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
    // 슬라이더 필드를 public으로 유지하되, Inspector에서 직접 할당하지 않아도 되도록 합니다.
    // public Slider BGMSlider; // 이제 자동 연결할 것이므로 Inspector에서 제거해도 됩니다.
    // public Slider SFXSlider; // 이제 자동 연결할 것이므로 Inspector에서 제거해도 됩니다.

    // private 변수로 변경하고 필요할 때 찾아 할당합니다.
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
                Debug.LogWarning("중복된 SFX 이름: " + sfx.sfxName + ". 첫 번째 항목만 사용됩니다.");
                continue;
            }
            sfxSoundDictionary.Add(sfx.sfxName, sfx);
        }
    }

    void OnEnable()
    {
        // 씬 로드 이벤트 구독: 새로운 씬이 로드될 때마다 OnSceneLoaded 함수가 호출됩니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화되거나 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 완료 시 호출될 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 '{scene.name}' 로드 완료. UI 슬라이더를 다시 연결합니다.");
        ConnectSliders(); // 슬라이더 연결 시도
    }

    // 슬라이더를 찾아 연결하고, 초기 볼륨을 설정하는 함수
    private void ConnectSliders()
    {
        // 씬에서 "BGMSlider"라는 이름의 Slider 컴포넌트를 찾습니다. (이름은 실제 UI 오브젝트 이름과 일치해야 합니다)
        _bgmSlider = FindObjectOfType<Slider>(); // 특정 이름으로 찾으려면 GameObject.Find("BGMSlider").GetComponent<Slider>();
                                                 // 이 방법은 씬에 슬라이더가 하나만 있거나, 특정 태그 등으로 구분할 수 있을 때 유리합니다.
                                                 // 예를 들어, UI 캔버스 내에 슬라이더가 있다면 더 구체적인 경로를 사용할 수 있습니다:
                                                 // Transform canvasTransform = GameObject.Find("Canvas").transform;
                                                 // _bgmSlider = canvasTransform.Find("BGMSlider").GetComponent<Slider>();


        // SFX 슬라이더 찾기
        // FindObjectsOfType<Slider>()를 사용하여 모든 슬라이더를 찾은 뒤,
        // 이름으로 구분하는 것이 일반적입니다.
        Slider[] allSliders = FindObjectsOfType<Slider>(true); // 비활성화된 오브젝트도 찾기 위해 true 추가
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


        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveAllListeners(); // 모든 리스너 제거
            _bgmSlider.onValueChanged.AddListener(SetBGMVolume); // BGM 전용 함수 연결
        }
        else
        {
            Debug.LogWarning("BGMSlider를 씬에서 찾을 수 없습니다! 이름이 'BGMSlider'인지 확인하세요.");
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveAllListeners(); // 모든 리스너 제거
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume); // SFX 전용 함수 연결
        }
        else
        {
            Debug.LogWarning("SFXSlider를 씬에서 찾을 수 없습니다! 이름이 'SFXSlider'인지 확인하세요.");
        }

        if (_bgmSlider != null) SetBGMVolume(_bgmSlider.value);
        if (_sfxSlider != null) SetSFXVolume(_sfxSlider.value);
    }

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
        Debug.Log($"BGM Slider Value: {sliderValue}, BGM Volume dB: {volumeDb}");
    }

    // 변경된 부분: 슬라이더 값을 매개변수로 직접 받습니다.
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
        Debug.Log($"SFX Slider Value: {sliderValue}, SFX Volume dB: {volumeDb}");
    }
    // 이 함수는 이제 내부적으로 _bgmSlider와 _sfxSlider를 참조합니다.
    public void AduioControl()
    {
        if (Mixer == null)
        {
            Debug.LogError("AudioMixer가 MainSoundManager에 할당되지 않았습니다!");
            return;
        }
        // 이제 필드를 사용하지 않고, private 변수를 사용합니다.
        if (_bgmSlider == null)
        {
            // Debug.LogError("BGMSlider가 아직 연결되지 않았습니다!"); // 연결이 실패했거나 아직 UI가 로드되지 않은 경우 발생
            return;
        }
        if (_sfxSlider == null)
        {
            // Debug.LogError("SFXSlider가 아직 연결되지 않았습니다!"); // 연결이 실패했거나 아직 UI가 로드되지 않은 경우 발생
            return;
        }

        // BGM 볼륨 조절
        float bgmSliderValue = _bgmSlider.value; // _bgmSlider 사용
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

        // SFX 볼륨 조절
        float sfxSliderValue = _sfxSlider.value; // _sfxSlider 사용
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