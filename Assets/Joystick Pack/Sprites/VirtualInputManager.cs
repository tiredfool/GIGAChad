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

    // �ܺ� (UI ��ư, ���̽�ƽ)���� SetButtonDownExternal/SetButtonUpExternal�� ȣ��� ��
    // �ش� ���� ��ư�� Down/Up �̺�Ʈ�� ������ �����ϱ� ���� �ӽ� �����
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
        // 1. ���� �������� Down/Up ���� �ʱ�ȭ
        // �� �κ��� GetKeyDown/Up�� �� �����Ӹ� true�� �����ϱ� ���� �ʼ��Դϴ�.
        foreach (var key in new List<string>(_buttonDownThisFrame.Keys))
        {
            _buttonDownThisFrame[key] = false;
        }
        foreach (var key in new List<string>(_buttonUpThisFrame.Keys))
        {
            _buttonUpThisFrame[key] = false;
        }

        // �߿�: _buttonIsPressed ���¸� Ű���� �Է� ���� ����� �Ӵϴ�.
        // �ܺ� �Է¿� ���� ���� ���¸� ���� Update���� �ݿ��ϱ� ����
        Dictionary<string, bool> previousFrameIsPressed = new Dictionary<string, bool>(_buttonIsPressed); // �� �κе� �߿�

        // 2. Ű���� �Է� ó�� �� ���� �������� Down/Up/Pressed ���� �ӽ� ���
        foreach (var mapping in keyMappings)
        {
            string buttonName = mapping.virtualButtonName;
            EnsureButtonExists(buttonName);

            // Ű���� KeyDown �̺�Ʈ
            if (Input.GetKeyDown(mapping.actualKey))
            {
                _buttonDownThisFrame[buttonName] = true;
                _buttonIsPressed[buttonName] = true; // Ű����� ������ �ٷ� Pressed ���·� ����
                Debug.Log($"[VIM-Keyboard] KeyDown: {mapping.actualKey} -> Virtual: {buttonName} DOWN");
            }
            // Ű���� KeyUp �̺�Ʈ
            if (Input.GetKeyUp(mapping.actualKey))
            {
                _buttonUpThisFrame[buttonName] = true;
                _buttonIsPressed[buttonName] = false; // Ű����� ���� �ٷ� Pressed ���¸� ����
                Debug.Log($"[VIM-Keyboard] KeyUp: {mapping.actualKey} -> Virtual: {buttonName} UP");
            }
            // Ű���� GetKey (�������� ����) ����
            // KeyDown/KeyUp���� ����� ���¸� �����ϰų�, �����ִ� ���¸� �ݿ�
            // ���� KeyDown�� �־��ٸ� _buttonIsPressed�� �̹� true�� ���̹Ƿ� �� �κ��� ����� �ʽ��ϴ�.
            // ������ ����ڰ� KeyDown �̺�Ʈ ���� �̹� Ű�� ������ �ִ� ���·� ������ ������ ��츦 ����մϴ�.
            if (Input.GetKey(mapping.actualKey))
            {
                _buttonIsPressed[buttonName] = true;
            }
        }

        // 3. �ܺ� ��û (���̽�ƽ, UI ��ư ��) ó�� �� ���� _buttonIsPressed ���� ����
        // �� �κ��� �ٽ�: Ű���� �Է°� �ܺ� ��û�� �����Ͽ� ���� _buttonIsPressed�� �����մϴ�.
        foreach (var buttonName in _buttonIsPressed.Keys.ToList())
        {
            bool hadExternalDownRequest = _externalButtonDownRequests.Contains(buttonName);
            bool hadExternalUpRequest = _externalButtonUpRequests.Contains(buttonName);

            // 1. Ű���忡 ���� ���� �����ִ°�?
            bool isPressedByKeyboard = keyMappings.Any(mapping => mapping.virtualButtonName == buttonName && Input.GetKey(mapping.actualKey));

            // 2. �ܺ� ��û�� ���� �����ִ� �����ΰ�?
            //    _buttonIsPressed�� ���� ���¸� �����մϴ�.
            if (isPressedByKeyboard || hadExternalDownRequest)
            {
                // Ű����� �����ְų� �ܺο��� Down ��û�� �־��ٸ� true
                _buttonIsPressed[buttonName] = true;

                // �ܺ� Down ��û�� �־���, �������� �������� �ʾҴٸ� Down �̺�Ʈ �߻�
                if (hadExternalDownRequest && !previousFrameIsPressed[buttonName])
                {
                    _buttonDownThisFrame[buttonName] = true;
                    // Debug.Log($"[VIM-External] ButtonDown: {buttonName} DOWN (from external request)");
                }
            }
            else if (hadExternalUpRequest)
            {
                // �ܺο��� Up ��û�� �־��ٸ� false
                _buttonIsPressed[buttonName] = false;

                // �ܺ� Up ��û�� �־���, �������� �����־��ٸ� Up �̺�Ʈ �߻�
                if (hadExternalUpRequest && previousFrameIsPressed[buttonName])
                {
                    _buttonUpThisFrame[buttonName] = true;
                    // Debug.Log($"[VIM-External] ButtonUp: {buttonName} UP (from external request)");
                }
            }
            else // Ű���� �Էµ� ����, �ܺ� ��û�� ���� �� (���� ���¸� �����ϰų�, �������� �ʴٸ� false ����)
            {
                // Ű���� �Է��� ���ٸ�, ���� ���°� true������ �� �����ӿ��� false�� �˴ϴ�.
                // (��, Ű����� �ܺε� ��� ������ �ʴ´ٸ� false)
                _buttonIsPressed[buttonName] = false;
            }
        }

        // 4. ���� �������� ���� �ܺ� ��û �ʱ�ȭ
        _externalButtonDownRequests.Clear();
        _externalButtonUpRequests.Clear();

        // --- ����� �α� (�ʿ�� Ȱ��ȭ) ---
        // if (GetKeyOrButton("Left")) Debug.Log("Left Virtual Button is currently PRESSED.");
        // if (GetKeyOrButtonDown("Left")) Debug.Log("Left Virtual Button DOWN detected!");
        // if (GetKeyOrButtonUp("Left")) Debug.Log("Left Virtual Button UP detected!");
    }

    // --- ���� �Լ�: ��ųʸ��� ��ư �̸��� ������ �ʱ�ȭ ---
    private void EnsureButtonExists(string buttonName)
    {
        if (!_buttonIsPressed.ContainsKey(buttonName))
        {
            _buttonIsPressed.Add(buttonName, false);
            _buttonDownThisFrame.Add(buttonName, false);
            _buttonUpThisFrame.Add(buttonName, false);
        }
    }

    // --- �ܺ� (UI ��ư, ���̽�ƽ ��)���� ȣ�� ������ ���� Ű/��ư ���� ���� �Լ� ---
    // �� �Լ��� Update() ������ ������ ��� ���¸� �����ϴ� ���� �ƴ϶�,
    // ���� Update() �ֱ⿡�� �ݿ��� '��û'�� ����ϴ� ������ �մϴ�.
    public void SetButtonDownExternal(string buttonName)
    {
        EnsureButtonExists(buttonName);
        _externalButtonDownRequests.Add(buttonName);
        _externalButtonUpRequests.Remove(buttonName); // ���ÿ� Up ��û�� ���� �� �����Ƿ�
        // Debug.Log($"[VIM-External Request] SetButtonDown: {buttonName}");
    }

    public void SetButtonUpExternal(string buttonName)
    {
        EnsureButtonExists(buttonName);
        _externalButtonUpRequests.Add(buttonName);
        _externalButtonDownRequests.Remove(buttonName); // ���ÿ� Down ��û�� ���� �� �����Ƿ�
        // Debug.Log($"[VIM-External Request] SetButtonUp: {buttonName}");
    }

    // --- �ܺο��� ȣ�� ������ ���� Ű/��ư ���� Ȯ�� �Լ� ---
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