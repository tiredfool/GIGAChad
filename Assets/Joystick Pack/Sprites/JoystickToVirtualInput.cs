using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class JoystickToVirtualInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float threshold = 0.1f; // 조이스틱 입력이 이 값 이상일 때만 가상 버튼을 누른 것으로 간주
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    private Vector2 joystickStartPos;
    private Vector2 inputVector; // 현재 조이스틱의 입력 벡터 (OnDrag에서 업데이트)

    private bool isTouchingJoystick = false; // 조이스틱이 터치되고 있는 중인지 여부

    void Start()
    {
        if (joystickHandle != null && joystickBackground != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
        // VirtualInputManager 인스턴스 확인
        if (VirtualInputManager.Instance == null)
        {
            Debug.LogError("VirtualInputManager 인스턴스를 찾을 수 없습니다. 씬에 VirtualInputManager가 있는지 확인해주세요.");
            return;
        }

        // 조이스틱이 터치되고 있지 않거나 (isTouchingJoystick == false),
        // 터치 중이지만 inputVector가 threshold 범위 안에 있을 때 (즉, 중앙에 있을 때)
        // -> 모든 조이스틱 관련 가상 버튼을 떼어짐 상태로 보냅니다.
        if (!isTouchingJoystick || (Mathf.Abs(inputVector.x) < threshold && Mathf.Abs(inputVector.y) < threshold))
        {
            VirtualInputManager.Instance.SetButtonUpExternal("Left");
            VirtualInputManager.Instance.SetButtonUpExternal("Right");
            VirtualInputManager.Instance.SetButtonUpExternal("Up");   // 추가: Up 버튼 해제
            VirtualInputManager.Instance.SetButtonUpExternal("Down"); // 추가: Down 버튼 해제
            // Debug.Log("[JoystickToVirtualInput Update] Sending ALL BUTTONS UP due to no touch or central position.");
        }
        else // 조이스틱이 터치 중이고, threshold를 넘어선 입력이 있을 때
        {
            // === 수평 입력 (Left/Right) 처리 ===
            if (inputVector.x < -threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Left");
                VirtualInputManager.Instance.SetButtonUpExternal("Right"); // 다른 방향은 떼기
                // Debug.Log("[JoystickToVirtualInput Update] Sending Left DOWN.");
            }
            else if (inputVector.x > threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Right");
                VirtualInputManager.Instance.SetButtonUpExternal("Left"); // 다른 방향은 떼기
                // Debug.Log("[JoystickToVirtualInput Update] Sending Right DOWN.");
            }
            else // 좌우 입력이 threshold 안에 있을 때 (이전 프레임에 눌렸더라도 떼어짐)
            {
                VirtualInputManager.Instance.SetButtonUpExternal("Left");
                VirtualInputManager.Instance.SetButtonUpExternal("Right");
                // Debug.Log("[JoystickToVirtualInput Update] Sending Left/Right UP (in threshold).");
            }

            // === 수직 입력 (Up/Down) 처리 ===
            if (inputVector.y > threshold)
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Up");
                VirtualInputManager.Instance.SetButtonUpExternal("Down"); // 다른 방향은 떼기
                // Debug.Log("[JoystickToVirtualInput Update] Sending Up DOWN.");
            }
            else if (inputVector.y < -threshold) // 추가: Down 버튼 처리
            {
                VirtualInputManager.Instance.SetButtonDownExternal("Down");
                VirtualInputManager.Instance.SetButtonUpExternal("Up");   // 다른 방향은 떼기
                // Debug.Log("[JoystickToVirtualInput Update] Sending Down DOWN.");
            }
            else // 상하 입력이 threshold 안에 있을 때 (이전 프레임에 눌렸더라도 떼어짐)
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
        isTouchingJoystick = true; // 터치 시작
        // OnDrag가 아직 호출되지 않았을 때를 대비하여 inputVector를 초기화하지 않습니다.
        // 첫 OnDrag에서 계산됩니다.
        // Debug.Log("[JoystickToVirtualInput] OnPointerDown");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out currentPos);

        // inputVector 계산 및 정규화
        Vector2 rawInput = currentPos - joystickStartPos;
        float distance = rawInput.magnitude;

        float backgroundRadius = joystickBackground.rect.width / 2f;

        // 핸들 위치 제한
        if (distance > backgroundRadius)
        {
            inputVector = rawInput.normalized; // 핸들이 배경 밖으로 나갔을 경우, 방향만 유지
            joystickHandle.anchoredPosition = inputVector * backgroundRadius;
        }
        else
        {
            inputVector = rawInput / backgroundRadius; // 배경 내에서는 상대적인 위치를 0~1 사이로 정규화
            joystickHandle.anchoredPosition = rawInput;
        }

        // inputVector의 최종 크기를 다시 정규화하여 항상 -1~1 범위가 되도록 보장
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);


        // 디버그 로그 (OnDrag에서 inputVector가 어떻게 변하는지 확인)
        // Debug.Log($"[JoystickToVirtualInput OnDrag] inputVector: {inputVector}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickHandle.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero; // 조이스틱 떼면 입력 벡터 초기화
        isTouchingJoystick = false; // 터치 끝
        // Debug.Log("[JoystickToVirtualInput] OnPointerUp");
        // Update에서 SetButtonUpExternal을 처리하므로, 여기서는 특별히 호출하지 않아도 됩니다.
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }
}