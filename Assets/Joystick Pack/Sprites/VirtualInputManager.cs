// VirtualInputManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class VirtualInputManager : MonoBehaviour
{
    public static VirtualInputManager Instance { get; private set; }

    private Dictionary<string, bool> _buttonIsPressed = new Dictionary<string, bool>();
    private Dictionary<string, bool> _buttonDownThisFrame = new Dictionary<string, bool>();
    private Dictionary<string, bool> _buttonUpThisFrame = new Dictionary<string, bool>();

    // 외부 (UI 버튼, 조이스틱)에서 SetButtonDownExternal/SetButtonUpExternal이 호출될 때
    // 해당 가상 버튼의 Down/Up 이벤트를 강제로 설정하기 위한 임시 저장소
    private HashSet<string> _externalButtonDownRequests = new HashSet<string>();
    private HashSet<string> _externalButtonUpRequests = new HashSet<string>();

    [System.Serializable]
    public class KeyToVirtualButton
    {
        public KeyCode actualKey;
        public string virtualButtonName;
    }

    public List<KeyToVirtualButton> keyMappings = new List<KeyToVirtualButton>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            foreach (var mapping in keyMappings)
            {
                EnsureButtonExists(mapping.virtualButtonName);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 1. 이전 프레임의 Down/Up 상태 초기화
        // 이 부분은 GetKeyDown/Up을 한 프레임만 true로 유지하기 위해 필수입니다.
        foreach (var key in new List<string>(_buttonDownThisFrame.Keys))
        {
            _buttonDownThisFrame[key] = false;
        }
        foreach (var key in new List<string>(_buttonUpThisFrame.Keys))
        {
            _buttonUpThisFrame[key] = false;
        }

        // 중요: _buttonIsPressed 상태를 키보드 입력 전에 백업해 둡니다.
        // 외부 입력에 의한 지속 상태를 다음 Update에서 반영하기 위함
        Dictionary<string, bool> previousFrameIsPressed = new Dictionary<string, bool>(_buttonIsPressed); // 이 부분도 중요

        // 2. 키보드 입력 처리 및 현재 프레임의 Down/Up/Pressed 상태 임시 기록
        foreach (var mapping in keyMappings)
        {
            string buttonName = mapping.virtualButtonName;
            EnsureButtonExists(buttonName);

            // 키보드 KeyDown 이벤트
            if (Input.GetKeyDown(mapping.actualKey))
            {
                _buttonDownThisFrame[buttonName] = true;
                _buttonIsPressed[buttonName] = true; // 키보드로 눌리면 바로 Pressed 상태로 만듦
                Debug.Log($"[VIM-Keyboard] KeyDown: {mapping.actualKey} -> Virtual: {buttonName} DOWN");
            }
            // 키보드 KeyUp 이벤트
            if (Input.GetKeyUp(mapping.actualKey))
            {
                _buttonUpThisFrame[buttonName] = true;
                _buttonIsPressed[buttonName] = false; // 키보드로 떼면 바로 Pressed 상태를 해제
                Debug.Log($"[VIM-Keyboard] KeyUp: {mapping.actualKey} -> Virtual: {buttonName} UP");
            }
            // 키보드 GetKey (지속적인 눌림) 상태
            // KeyDown/KeyUp으로 변경된 상태를 유지하거나, 눌려있는 상태를 반영
            // 이전 KeyDown이 있었다면 _buttonIsPressed는 이미 true일 것이므로 이 부분은 덮어쓰지 않습니다.
            // 하지만 사용자가 KeyDown 이벤트 없이 이미 키를 누르고 있는 상태로 게임을 시작할 경우를 대비합니다.
            if (Input.GetKey(mapping.actualKey))
            {
                _buttonIsPressed[buttonName] = true;
            }
        }

        // 3. 외부 요청 (조이스틱, UI 버튼 등) 처리 및 최종 _buttonIsPressed 상태 결정
        // 이 부분이 핵심: 키보드 입력과 외부 요청을 통합하여 최종 _buttonIsPressed를 결정합니다.
        foreach (var buttonName in _buttonIsPressed.Keys.ToList())
        {
            bool hadExternalDownRequest = _externalButtonDownRequests.Contains(buttonName);
            bool hadExternalUpRequest = _externalButtonUpRequests.Contains(buttonName);

            // 1. 키보드에 의해 현재 눌려있는가?
            bool isPressedByKeyboard = keyMappings.Any(mapping => mapping.virtualButtonName == buttonName && Input.GetKey(mapping.actualKey));

            // 2. 외부 요청에 의해 눌려있는 상태인가?
            //    _buttonIsPressed의 최종 상태를 결정합니다.
            if (isPressedByKeyboard || hadExternalDownRequest)
            {
                // 키보드로 눌려있거나 외부에서 Down 요청이 있었다면 true
                _buttonIsPressed[buttonName] = true;

                // 외부 Down 요청이 있었고, 이전에는 눌려있지 않았다면 Down 이벤트 발생
                if (hadExternalDownRequest && !previousFrameIsPressed[buttonName])
                {
                    _buttonDownThisFrame[buttonName] = true;
                    // Debug.Log($"[VIM-External] ButtonDown: {buttonName} DOWN (from external request)");
                }
            }
            else if (hadExternalUpRequest)
            {
                // 외부에서 Up 요청이 있었다면 false
                _buttonIsPressed[buttonName] = false;

                // 외부 Up 요청이 있었고, 이전에는 눌려있었다면 Up 이벤트 발생
                if (hadExternalUpRequest && previousFrameIsPressed[buttonName])
                {
                    _buttonUpThisFrame[buttonName] = true;
                    // Debug.Log($"[VIM-External] ButtonUp: {buttonName} UP (from external request)");
                }
            }
            else // 키보드 입력도 없고, 외부 요청도 없을 때 (이전 상태를 유지하거나, 눌려있지 않다면 false 유지)
            {
                // 키보드 입력이 없다면, 이전 상태가 true였더라도 이 프레임에는 false가 됩니다.
                // (즉, 키보드든 외부든 계속 눌리지 않는다면 false)
                _buttonIsPressed[buttonName] = false;
            }
        }

        // 4. 다음 프레임을 위한 외부 요청 초기화
        _externalButtonDownRequests.Clear();
        _externalButtonUpRequests.Clear();

        // --- 디버그 로그 (필요시 활성화) ---
        // if (GetKeyOrButton("Left")) Debug.Log("Left Virtual Button is currently PRESSED.");
        // if (GetKeyOrButtonDown("Left")) Debug.Log("Left Virtual Button DOWN detected!");
        // if (GetKeyOrButtonUp("Left")) Debug.Log("Left Virtual Button UP detected!");
    }

    // --- 헬퍼 함수: 딕셔너리에 버튼 이름이 없으면 초기화 ---
    private void EnsureButtonExists(string buttonName)
    {
        if (!_buttonIsPressed.ContainsKey(buttonName))
        {
            _buttonIsPressed.Add(buttonName, false);
            _buttonDownThisFrame.Add(buttonName, false);
            _buttonUpThisFrame.Add(buttonName, false);
        }
    }

    // --- 외부 (UI 버튼, 조이스틱 등)에서 호출 가능한 가상 키/버튼 상태 설정 함수 ---
    // 이 함수는 Update() 로직과 별개로 즉시 상태를 변경하는 것이 아니라,
    // 다음 Update() 주기에서 반영될 '요청'을 기록하는 역할을 합니다.
    public void SetButtonDownExternal(string buttonName)
    {
        EnsureButtonExists(buttonName);
        _externalButtonDownRequests.Add(buttonName);
        _externalButtonUpRequests.Remove(buttonName); // 동시에 Up 요청이 있을 수 없으므로
        // Debug.Log($"[VIM-External Request] SetButtonDown: {buttonName}");
    }

    public void SetButtonUpExternal(string buttonName)
    {
        EnsureButtonExists(buttonName);
        _externalButtonUpRequests.Add(buttonName);
        _externalButtonDownRequests.Remove(buttonName); // 동시에 Down 요청이 있을 수 없으므로
        // Debug.Log($"[VIM-External Request] SetButtonUp: {buttonName}");
    }

    // --- 외부에서 호출 가능한 가상 키/버튼 상태 확인 함수 ---
    public bool GetKeyOrButton(string buttonName)
    {
        EnsureButtonExists(buttonName);
        return _buttonIsPressed[buttonName];
    }

    public bool GetKeyOrButtonDown(string buttonName)
    {
        EnsureButtonExists(buttonName);
        return _buttonDownThisFrame[buttonName];
    }

    public bool GetKeyOrButtonUp(string buttonName)
    {
        EnsureButtonExists(buttonName);
        return _buttonUpThisFrame[buttonName];
    }
    public void off()
    {
        Instance = null;
        Destroy(this.gameObject);
    }
}