using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class JoystickToVirtualInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float threshold = 0.1f; // ���̽�ƽ �Է��� �� �� �̻��� ���� ���� ��ư�� ���� ������ ����
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    private Vector2 joystickStartPos;
    private Vector2 inputVector; // ���� ���̽�ƽ�� �Է� ���� (OnDrag���� ������Ʈ)

    private bool isTouchingJoystick = false; // ���̽�ƽ�� ��ġ�ǰ� �ִ� ������ ����

    void Start()
    {
        if (joystickHandle != null && joystickBackground != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
        // VirtualInputManager �ν��Ͻ� Ȯ��
        if (VirtualInputManager.Instance == null)
        {
            Debug.LogError("VirtualInputManager �ν��Ͻ��� ã�� �� �����ϴ�. ���� VirtualInputManager�� �ִ��� Ȯ�����ּ���.");
            return;
        }

        // ���̽�ƽ�� ��ġ�ǰ� ���� �ʰų� (isTouchingJoystick == false),
        // ��ġ �������� inputVector�� threshold ���� �ȿ� ���� �� (��, �߾ӿ� ���� ��)
        // -> ��� ���̽�ƽ ���� ���� ��ư�� ������ ���·� �����ϴ�.
        if (!isTouchingJoystick || (Mathf.Abs(inputVector.x) < threshold && Mathf.Abs(inputVector.y) < threshold))
        {
            VirtualInputManager.Instance.SetButtonUpExternal("Left");
            VirtualInputManager.Instance.SetButtonUpExternal("Right");
            VirtualInputManager.Instance.SetButtonUpExternal("Up");   // �߰�: Up ��ư ����
            VirtualInputManager.Instance.SetButtonUpExternal("Down"); // �߰�: Down ��ư ����
            // Debug.Log("[JoystickToVirtualInput Update] Sending ALL BUTTONS UP due to no touch or central position.");
        }
        else // ���̽�ƽ�� ��ġ ���̰�, threshold�� �Ѿ �Է��� ���� ��
        {
            // === ���� �Է� (Left/Right) ó�� ===
            if (inputVector.x < -threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Left");
                VirtualInputManager.Instance.SetButtonUpExternal("Right"); // �ٸ� ������ ����
                // Debug.Log("[JoystickToVirtualInput Update] Sending Left DOWN.");
            }
            else if (inputVector.x > threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Right");
                VirtualInputManager.Instance.SetButtonUpExternal("Left"); // �ٸ� ������ ����
                // Debug.Log("[JoystickToVirtualInput Update] Sending Right DOWN.");
            }
            else // �¿� �Է��� threshold �ȿ� ���� �� (���� �����ӿ� ���ȴ��� ������)
            {
                VirtualInputManager.Instance.SetButtonUpExternal("Left");
                VirtualInputManager.Instance.SetButtonUpExternal("Right");
                // Debug.Log("[JoystickToVirtualInput Update] Sending Left/Right UP (in threshold).");
            }

            // === ���� �Է� (Up/Down) ó�� ===
            if (inputVector.y > threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Up");
                VirtualInputManager.Instance.SetButtonUpExternal("Down"); // �ٸ� ������ ����
                // Debug.Log("[JoystickToVirtualInput Update] Sending Up DOWN.");
            }
            else if (inputVector.y < -threshold) // �߰�: Down ��ư ó��
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Down");
                VirtualInputManager.Instance.SetButtonUpExternal("Up");   // �ٸ� ������ ����
                // Debug.Log("[JoystickToVirtualInput Update] Sending Down DOWN.");
            }
            else // ���� �Է��� threshold �ȿ� ���� �� (���� �����ӿ� ���ȴ��� ������)
            {
                VirtualInputManager.Instance.SetButtonUpExternal("Up");
                VirtualInputManager.Instance.SetButtonUpExternal("Down");
                // Debug.Log("[JoystickToVirtualInput Update] Sending Up/Down UP (in threshold).");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out joystickStartPos);
        isTouchingJoystick = true; // ��ġ ����
        // OnDrag�� ���� ȣ����� �ʾ��� ���� ����Ͽ� inputVector�� �ʱ�ȭ���� �ʽ��ϴ�.
        // ù OnDrag���� ���˴ϴ�.
        // Debug.Log("[JoystickToVirtualInput] OnPointerDown");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out currentPos);

        // inputVector ��� �� ����ȭ
        Vector2 rawInput = currentPos - joystickStartPos;
        float distance = rawInput.magnitude;

        float backgroundRadius = joystickBackground.rect.width / 2f;

        // �ڵ� ��ġ ����
        if (distance > backgroundRadius)
        {
            inputVector = rawInput.normalized; // �ڵ��� ��� ������ ������ ���, ���⸸ ����
            joystickHandle.anchoredPosition = inputVector * backgroundRadius;
        }
        else
        {
            inputVector = rawInput / backgroundRadius; // ��� �������� ������� ��ġ�� 0~1 ���̷� ����ȭ
            joystickHandle.anchoredPosition = rawInput;
        }

        // inputVector�� ���� ũ�⸦ �ٽ� ����ȭ�Ͽ� �׻� -1~1 ������ �ǵ��� ����
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);


        // ����� �α� (OnDrag���� inputVector�� ��� ���ϴ��� Ȯ��)
        // Debug.Log($"[JoystickToVirtualInput OnDrag] inputVector: {inputVector}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickHandle.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero; // ���̽�ƽ ���� �Է� ���� �ʱ�ȭ
        isTouchingJoystick = false; // ��ġ ��
        // Debug.Log("[JoystickToVirtualInput] OnPointerUp");
        // Update���� SetButtonUpExternal�� ó���ϹǷ�, ���⼭�� Ư���� ȣ������ �ʾƵ� �˴ϴ�.
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }
}