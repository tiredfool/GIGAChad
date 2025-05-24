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

    // BGM�� ���� ���ο� Ŭ����
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
    private Dictionary<string, BGMSound> bgmSoundDictionary = new Dictionary<string, BGMSound>(); // BGM ��ųʸ� �߰�
    private Dictionary<string, AudioSource> activeSfxAudioSources = new Dictionary<string, AudioSource>();

    private AudioSource bgmAudioSource;
    public AudioMixer Mixer;

    
    private Slider _bgmSlider; // �ڵ����� �Ҵ�
    private Slider _sfxSlider;

    private const float MIN_VOLUME_DB = -80f; // AudioMixer�� ���� ���� ����
    private const float MAX_VOLUME_DB = 0f;   // AudioMixer�� �⺻ ���� (����� Ŭ���� ���� ����)

    //�ʱ⺼��
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
                Debug.LogWarning("�ߺ��� SFX �̸�: " + sfx.sfxName + ". ù ��° �׸� ���˴ϴ�.");
                continue;
            }
            sfxSoundDictionary.Add(sfx.sfxName, sfx);
        }
        foreach (BGMSound bgm in bgmSoundsList)
        {
            if (bgmSoundDictionary.ContainsKey(bgm.bgmName))
            {
                Debug.LogWarning("�ߺ��� BGM �̸�: " + bgm.bgmName + ". ù ��° �׸� ���˴ϴ�.");
                continue;
            }
            bgmSoundDictionary.Add(bgm.bgmName, bgm);
        }
        // BGM AudioSource �ʱ�ȭ
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true; // BGM�� �⺻������ �ݺ� ���
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
                Debug.LogWarning("AudioMixer���� 'BGM' �׷��� ã�� �� �����ϴ�. BGM AudioSource�� �ͼ��� ������� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("MainSoundManager�� Mixer�� �Ҵ���� �ʾ� BGM AudioSource�� �ͼ��� ������ �� �����ϴ�.");
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
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = currentBGMVolume;
        }
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

    public void PlayBGM(string bgmName)
    {
        if (bgmSoundDictionary.TryGetValue(bgmName, out BGMSound bgm))
        {
            if (bgmAudioSource == null)
            {
                Debug.LogError("BGM AudioSource�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
                return;
            }

            // ���� ��� ���� BGM�� ��û�� BGM�� ����, �̹� ��� ���̶�� �ߺ� ��� ����
            if (bgmAudioSource.clip == bgm.bgmClip && bgmAudioSource.isPlaying)
            {
                Debug.Log($"BGM '{bgmName}'��(��) �̹� ��� ���Դϴ�.");
                return;
            }

            bgmAudioSource.clip = bgm.bgmClip;
            bgmAudioSource.volume = bgm.volume; // BGM ���� ���� ����
            bgmAudioSource.Play();
            Debug.Log($"BGM '{bgmName}' ��� ����. Ŭ�� ����: {bgm.volume}");
        }
        else
        {
            Debug.LogWarning($"BGM '{bgmName}'�� ã�� �� �����ϴ�. MainSoundManager�� bgmSoundsList�� Ȯ���ϼ���.");
        }
    }
    public void StopBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log("BGM ����.");
        }
    }

    public void ChangeBGM(string newBgmName, float fadeTime = 1.0f)
    {
        if (!bgmSoundDictionary.ContainsKey(newBgmName))
        {
            Debug.LogWarning($"�� BGM '{newBgmName}'�� ã�� �� �����ϴ�. ��ȯ���� �ʽ��ϴ�.");
            return;
        }

        BGMSound newBgm;
        bgmSoundDictionary.TryGetValue(newBgmName, out newBgm);

        // ���� ��� ���� BGM�� ��û�� BGM�� ����, �̹� ��� ���̶�� ��ȯ���� ����
        if (bgmAudioSource.clip == newBgm.bgmClip && bgmAudioSource.isPlaying)
        {
            Debug.Log($"BGM '{newBgmName}'��(��) �̹� ��� ���̹Ƿ� ��ȯ���� �ʽ��ϴ�.");
            return;
        }

        // �̹� BGM ��ȯ�� ���� ���̶�� ���� �ڷ�ƾ�� ����
        if (currentBGMChangeCoroutine != null)
        {
            StopCoroutine(currentBGMChangeCoroutine);
            currentBGMChangeCoroutine = null;
            Debug.Log("���� BGM ��ȯ �ڷ�ƾ ����.");
        }

        // BGM ��ȯ �ڷ�ƾ ����
        currentBGMChangeCoroutine = StartCoroutine(ChangeBGMCoroutine(newBgm, fadeTime));
    }

    private IEnumerator ChangeBGMCoroutine(BGMSound newBgm, float fadeTime)
    {
        // 1. ���� BGM ���̵� �ƿ�
        if (bgmAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutBGM(fadeTime));
        }

        // 2. ���ο� BGM ���� �� ���̵� �� ����
        bgmAudioSource.clip = newBgm.bgmClip;
        bgmAudioSource.volume = 0f; // ���̵� ���� ���� �ʱ� ���� 0���� ����
        bgmAudioSource.Play();
        bgmAudioSource.loop = true; // BGM�� �׻� ����

        yield return StartCoroutine(FadeInBGM(newBgm.volume, fadeTime)); // newBgm.volume�� Ŭ���� ���� ����
        currentBGMChangeCoroutine = null; // �ڷ�ƾ �Ϸ�
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmAudioSource.volume; // ���� BGM AudioSource�� ����
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            // Linear ����
            float newVolume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            bgmAudioSource.volume = newVolume;

            // AudioMixer ���� ���� (�ͼ� �׷� ������ ����)
            float mixerDb = (newVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(newVolume * currentBGMVolume) * 20; // UI �����̴� �� �ݿ�
            Mixer.SetFloat("BGM", mixerDb);

            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
        Mixer.SetFloat("BGM", MIN_VOLUME_DB); // ������ ���Ұ�
        Debug.Log("BGM ���̵� �ƿ� �Ϸ�.");
    }

    private IEnumerator FadeInBGM(float targetClipVolume, float fadeTime)
    {
        float timer = 0f;
        float startVolume = 0f; // ���̵� ���� 0���� ����

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            // Linear ����
            float newVolume = Mathf.Lerp(startVolume, targetClipVolume, timer / fadeTime);
            bgmAudioSource.volume = newVolume; // BGM AudioSource�� ����

            // AudioMixer ���� ���� (�ͼ� �׷� ������ ����)
            float mixerDb = (newVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(newVolume * currentBGMVolume) * 20; // UI �����̴� �� �ݿ�
            Mixer.SetFloat("BGM", mixerDb);

            yield return null;
        }

        bgmAudioSource.volume = targetClipVolume; // ���� �������� ���� (���� BGM�� ���� ����)
        // ���� �ͼ� ������ ����
        float finalMixerDb = (targetClipVolume <= 0.0001f) ? MIN_VOLUME_DB : Mathf.Log10(targetClipVolume * currentBGMVolume) * 20;
        Mixer.SetFloat("BGM", finalMixerDb);
        Debug.Log("BGM ���̵� �� �Ϸ�.");
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
    public void StopAllSFX()
    {
        Debug.Log("Stopping all SFX...");

        AudioSource[] allAudioSourcesOnThisObject = GetComponents<AudioSource>();

        foreach (AudioSource source in allAudioSourcesOnThisObject)
        {

            if (source != null && source.isPlaying && source != bgmAudioSource) // BGM AudioSource�� ����
            {
                source.Stop(); // AudioSource ��� ����
            }
            if (source != bgmAudioSource) // BGM AudioSource�� �ı����� ����
            {
                Destroy(source); // ������Ʈ �ı� (�޸� ����)
            }
        }

        activeSfxAudioSources.Clear();
        StopAllCoroutines(); // ��� �ڷ�ƾ ���� (SFX CleanUp �ڷ�ƾ ����)

    }

    public void off()
    {
        instance = null;
        Destroy(this.gameObject);
    }
}