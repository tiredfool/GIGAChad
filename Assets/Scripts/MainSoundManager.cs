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

    // BGM을 위한 새로운 클래스
    [System.Serializable]
    public class BGMSound
    {
        public string bgmName;
        public AudioClip bgmClip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public List<SFXSound> sfxSoundsList;
    public List<BGMSound> bgmSoundsList;
    private Dictionary<string, SFXSound> sfxSoundDictionary = new Dictionary<string, SFXSound>();
    private Dictionary<string, BGMSound> bgmSoundDictionary = new Dictionary<string, BGMSound>(); // BGM 딕셔너리 추가
    private Dictionary<string, AudioSource> activeSfxAudioSources = new Dictionary<string, AudioSource>();

    private AudioSource bgmAudioSource;
    public AudioMixer Mixer;

    
    private Slider _bgmSlider; // 자동으로 할당
    private Slider _sfxSlider;

    private const float MIN_VOLUME_DB = -80f; // AudioMixer의 가장 낮은 볼륨
    private const float MAX_VOLUME_DB = 0f;   // AudioMixer의 기본 볼륨 (오디오 클립의 원본 볼륨)

    //초기볼륨
    private float currentBGMVolume = 1f; 
    private float currentSFXVolume = 1f;

    private Coroutine currentBGMChangeCoroutine = null;
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
        foreach (BGMSound bgm in bgmSoundsList)
        {
            if (bgmSoundDictionary.ContainsKey(bgm.bgmName))
            {
                Debug.LogWarning("중복된 BGM 이름: " + bgm.bgmName + ". 첫 번째 항목만 사용됩니다.");
                continue;
            }
            bgmSoundDictionary.Add(bgm.bgmName, bgm);
        }
        // BGM AudioSource 초기화
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true; // BGM은 기본적으로 반복 재생
        bgmAudioSource.playOnAwake = false;

        if (Mixer != null)
        {
            AudioMixerGroup[] bgmGroups = Mixer.FindMatchingGroups("BGM");
            if (bgmGroups.Length > 0)
            {
                bgmAudioSource.outputAudioMixerGroup = bgmGroups[0];
            }
            else
            {
                Debug.LogWarning("AudioMixer에서 'BGM' 그룹을 찾을 수 없습니다. BGM AudioSource가 믹서에 연결되지 않습니다.");
            }
        }
        else
        {
            Debug.LogWarning("MainSoundManager의 Mixer가 할당되지 않아 BGM AudioSource를 믹서에 연결할 수 없습니다.");
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
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = currentBGMVolume;
        }
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

    public void PlayBGM(string bgmName)
    {
        if (bgmSoundDictionary.TryGetValue(bgmName, out BGMSound bgm))
        {
            if (bgmAudioSource == null)
            {
                Debug.LogError("BGM AudioSource가 초기화되지 않았습니다!");
                return;
            }

            // 현재 재생 중인 BGM과 요청된 BGM이 같고, 이미 재생 중이라면 중복 재생 방지
            if (bgmAudioSource.clip == bgm.bgmClip && bgmAudioSource.isPlaying)
            {
                Debug.Log($"BGM '{bgmName}'이(가) 이미 재생 중입니다.");
                return;
            }

            bgmAudioSource.clip = bgm.bgmClip;
            bgmAudioSource.volume = bgm.volume; // BGM 개별 볼륨 적용
            bgmAudioSource.Play();
            Debug.Log($"BGM '{bgmName}' 재생 시작. 클립 볼륨: {bgm.volume}");
        }
        else
        {
            Debug.LogWarning($"BGM '{bgmName}'를 찾을 수 없습니다. MainSoundManager의 bgmSoundsList를 확인하세요.");
        }
    }
    public void StopBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log("BGM 정지.");
        }
    }

    public void ChangeBGM(string newBgmName, float fadeTime = 1.0f)
    {
        if (!bgmSoundDictionary.ContainsKey(newBgmName))
        {
            Debug.LogWarning($"새 BGM '{newBgmName}'를 찾을 수 없습니다. 전환하지 않습니다.");
            return;
        }

        BGMSound newBgm;
        bgmSoundDictionary.TryGetValue(newBgmName, out newBgm);

        // 현재 재생 중인 BGM과 요청된 BGM이 같고, 이미 재생 중이라면 전환하지 않음
        if (bgmAudioSource.clip == newBgm.bgmClip && bgmAudioSource.isPlaying)
        {
            Debug.Log($"BGM '{newBgmName}'이(가) 이미 재생 중이므로 전환하지 않습니다.");
            return;
        }

        // 이미 BGM 전환이 진행 중이라면 기존 코루틴을 중지
        if (currentBGMChangeCoroutine != null)
        {
            StopCoroutine(currentBGMChangeCoroutine);
            currentBGMChangeCoroutine = null;
            Debug.Log("기존 BGM 전환 코루틴 중지.");
        }

        // BGM 전환 코루틴 시작
        currentBGMChangeCoroutine = StartCoroutine(ChangeBGMCoroutine(newBgm, fadeTime));
    }

    private IEnumerator ChangeBGMCoroutine(BGMSound newBgm, float fadeTime)
    {
        // 1. 현재 BGM 페이드 아웃
        if (bgmAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutBGM(fadeTime));
        }

        // 2. 새로운 BGM 설정 및 페이드 인 시작
        bgmAudioSource.clip = newBgm.bgmClip;
        bgmAudioSource.volume = 0f; // 페이드 인을 위해 초기 볼륨 0으로 설정
        bgmAudioSource.Play();
        bgmAudioSource.loop = true; // BGM은 항상 루프

        yield return StartCoroutine(FadeInBGM(newBgm.volume, fadeTime)); // newBgm.volume은 클립의 개별 볼륨
        currentBGMChangeCoroutine = null; // 코루틴 완료
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmAudioSource.volume; // 현재 BGM AudioSource의 볼륨
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            // Linear 감소
            float newVolume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            bgmAudioSource.volume = newVolume;

            // AudioMixer 볼륨 조절 (믹서 그룹 볼륨을 조절)
            float mixerDb = (newVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(newVolume * currentBGMVolume) * 20; // UI 슬라이더 값 반영
            Mixer.SetFloat("BGM", mixerDb);

            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
        Mixer.SetFloat("BGM", MIN_VOLUME_DB); // 완전히 음소거
        Debug.Log("BGM 페이드 아웃 완료.");
    }

    private IEnumerator FadeInBGM(float targetClipVolume, float fadeTime)
    {
        float timer = 0f;
        float startVolume = 0f; // 페이드 인은 0에서 시작

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            // Linear 증가
            float newVolume = Mathf.Lerp(startVolume, targetClipVolume, timer / fadeTime);
            bgmAudioSource.volume = newVolume; // BGM AudioSource의 볼륨

            // AudioMixer 볼륨 조절 (믹서 그룹 볼륨을 조절)
            float mixerDb = (newVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(newVolume * currentBGMVolume) * 20; // UI 슬라이더 값 반영
            Mixer.SetFloat("BGM", mixerDb);

            yield return null;
        }

        bgmAudioSource.volume = targetClipVolume; // 최종 볼륨으로 설정 (개별 BGM의 설정 볼륨)
        // 최종 믹서 볼륨도 설정
        float finalMixerDb = (targetClipVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(targetClipVolume * currentBGMVolume) * 20;
        Mixer.SetFloat("BGM", finalMixerDb);
        Debug.Log("BGM 페이드 인 완료.");
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
    public void StopAllSFX()
    {
        Debug.Log("Stopping all SFX...");

        AudioSource[] allAudioSourcesOnThisObject = GetComponents<AudioSource>();

        foreach (AudioSource source in allAudioSourcesOnThisObject)
        {

            if (source != null && source.isPlaying && source != bgmAudioSource) // BGM AudioSource는 제외
            {
                source.Stop(); // AudioSource 재생 중지
            }
            if (source != bgmAudioSource) // BGM AudioSource는 파괴하지 않음
            {
                Destroy(source); // 컴포넌트 파괴 (메모리 해제)
            }
        }

        activeSfxAudioSources.Clear();
        StopAllCoroutines(); // 모든 코루틴 중지 (SFX CleanUp 코루틴 포함)

    }

    public void off()
    {
        instance = null;
        Destroy(this.gameObject);
    }
}