using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class SwitchZone : MonoBehaviour
{
    //public static SwitchZone Instance { get; private set; }

    public GameObject platformerPlayer;
    public GameObject topDownPlayer;
    public CinemachineVirtualCamera vcamPlatformer;
    public CinemachineVirtualCamera vcamTopDown;
    public float switchDuration = 10f;
    public GameObject foodSpawner;

    public GameObject platformerUI;
    public GameObject topDownUI;
    public GameObject SoundManager;
    public GameObject dialogueEndTrigger;
    public GameObject dialogueStartTrigger;

    private bool isTopDown = false;

    private PlayerController platformerPlayerController;
    //private void Awake()
    //{
    //    // 싱글톤 설정
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Instance = this;
    //    DontDestroyOnLoad(gameObject); // 씬 유지
    //}

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator DelayedInit()
    {
        yield return null; // 1 프레임 대기
        TryFindReferences();
        InitState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("씬 로드됨: " + scene.name);
        StartCoroutine(DelayedInit());
    }

    private void TryFindReferences()
    {
        if (platformerPlayer == null)
        {
            platformerPlayer = GameObject.FindWithTag("Player");
            if (platformerPlayer == null)
                platformerPlayer = FindInactiveWithTag("Player");
        }
        if (topDownPlayer == null)
        {
            topDownPlayer = GameObject.FindWithTag("PlayerHidden");
            if (topDownPlayer == null)
                topDownPlayer = FindInactiveWithTag("PlayerHidden");
        }

        if (dialogueStartTrigger == null)
        {
            dialogueStartTrigger = GameObject.Find("dialogueHiddenS");
            if (dialogueStartTrigger == null)
                dialogueStartTrigger = FindInactiveWithTag("DHE");
        }

        if (dialogueEndTrigger == null)
        {
            dialogueEndTrigger = GameObject.FindWithTag("DHE");
            if (dialogueEndTrigger == null)
                dialogueEndTrigger = FindInactiveWithTag("DHE");
        }

        if (platformerPlayer != null && platformerPlayerController == null)
        {
            platformerPlayerController = platformerPlayer.GetComponent<PlayerController>();
            if (platformerPlayerController == null)
            {
                Debug.LogError("Platformer Player에 PlayerController 스크립트가 없습니다!");
            }
        }


        if (vcamPlatformer == null)
        {
            GameObject vcamObj = GameObject.Find("Virtual Camera");
            if (vcamObj != null)
                vcamPlatformer = vcamObj.GetComponent<CinemachineVirtualCamera>();
        }

        if (vcamTopDown == null)
        {
            GameObject vcamTD = GameObject.Find("Vcam_TopDown");
            if (vcamTD != null)
                vcamTopDown = vcamTD.GetComponent<CinemachineVirtualCamera>();
        }

        if (foodSpawner == null)
        {
            foodSpawner = GameObject.FindWithTag("FS");
            if (foodSpawner == null)
                foodSpawner = FindInactiveWithTag("FS");
        }



        if (platformerUI == null)
            platformerUI = GameObject.FindWithTag("PU");

        if (topDownUI == null)
            topDownUI = GameObject.FindWithTag("TU");
        if (MainSoundManager.instance != null)
            SoundManager = MainSoundManager.instance.gameObject;
        
    }

    private void InitState()
    {
        if (foodSpawner != null)
            foodSpawner.SetActive(false);
        if (topDownPlayer != null)
            topDownPlayer.SetActive(false);

        if (platformerUI != null)
            platformerUI.SetActive(true);
        if (topDownUI != null)
            topDownUI.SetActive(false);
        if(dialogueEndTrigger != null)
            dialogueEndTrigger.GetComponent<Collider2D>().enabled = false;
        if (dialogueStartTrigger != null)
            dialogueStartTrigger.GetComponent<Collider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == platformerPlayer && !isTopDown)
        {
            StartCoroutine(SwitchToTopDown());
        }
    }

    private IEnumerator SwitchToTopDown()
    {
        Debug.Log("SwitchToTopDown 코루틴 시작");
        isTopDown = true;
        Debug.Log("BGM 바꾸기 전");
        SoundManager.GetComponent<MainSoundManager>().ChangeBGM("1stage hidden");
        Debug.Log("BGM 바꿈");
        if (vcamPlatformer != null)
        {
            vcamPlatformer.Priority = 5;
            Debug.Log("플랫포머 카메라 priority 낮춤");
        }
        if (vcamTopDown != null)
        {
            vcamTopDown.Priority = 10;
            Debug.Log("톱다운 카메라 priority 높임");
        }

        //if (platformerUI != null) platformerUI.SetActive(false);
        if (topDownUI != null) topDownUI.SetActive(true);

        if (platformerPlayer != null) platformerPlayer.SetActive(false);

        if (platformerPlayerController != null) // 발소리 제거
        {
            platformerPlayerController.isPlayingFootstepSound = false;
            StopCoroutine("PlayFootstepSound");
            if (MainSoundManager.instance != null)
            {
                MainSoundManager.instance.StopSFX("Footstep");
            }
        }
        if (platformerPlayer != null) platformerPlayer.SetActive(false);
        if (topDownPlayer != null) topDownPlayer.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (dialogueStartTrigger != null)
            dialogueStartTrigger.GetComponent<Collider2D>().enabled = true;

        yield return new WaitUntil(() => dialogueStartTrigger == null);

        if (foodSpawner != null) foodSpawner.SetActive(true);

        Debug.Log("톱다운 모드 시작");

        yield return new WaitForSeconds(switchDuration);

        if (topDownPlayer != null)
        {
            Player player = topDownPlayer.GetComponent<Player>();
            if (player != null)
                player.EndMiniGame();
        }
        if (dialogueEndTrigger != null)
        {
            dialogueEndTrigger.GetComponent<Collider2D>().enabled = true; ;
            Debug.Log("end dialogue 오브젝트 활성화");
        }

        yield return new WaitUntil(() => dialogueEndTrigger == null);// 두번째 대화 끝날 때까지 대기

        Debug.Log("톱다운 모드 종료");

        if (topDownUI != null) topDownUI.SetActive(false);
        if (topDownPlayer != null) topDownPlayer.SetActive(false);
        if (foodSpawner != null) {
            // foodSpawner.s
            MonsterSpawner temp = foodSpawner.GetComponent<MonsterSpawner>();
            temp.stopSpawn();//인위적으로 꺼줘야함
            foodSpawner.SetActive(false); 
        }
        if (vcamTopDown != null) vcamTopDown.gameObject.SetActive(false);

        StartCoroutine(SwitchToPlatformer());

        gameObject.SetActive(false);
    }

    private IEnumerator SwitchToPlatformer()
    {
        Debug.Log("플랫포머 모드로 복귀");
        SoundManager.GetComponent<MainSoundManager>().ChangeBGM("Basic");
        isTopDown = false;

        if (vcamPlatformer != null) vcamPlatformer.Priority = 10;

        if (platformerPlayer != null) platformerPlayer.SetActive(true);
        if (platformerUI != null) platformerUI.SetActive(true);

        yield return null;
    }

    private GameObject FindInactiveWithTag(string tag)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true); // true → 비활성 포함
            foreach (Transform child in children)
            {
                if (child.CompareTag(tag))
                    return child.gameObject;
            }
        }
        return null;
    }
    public void off()
    {
        //Instance = null;
        Destroy(this.gameObject);
    }

}

