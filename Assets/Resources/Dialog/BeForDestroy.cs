using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 필요

// [System.Serializable] 속성을 추가하여 Inspector에 노출되도록 합니다.
// class 대신 struct를 사용하여 가볍게 만듭니다.
[System.Serializable]
public struct EventOnDestroy
{
    // Inspector에서 이 이벤트의 이름을 구분할 수 있도록 필드 추가 (선택 사항)
    public string eventName;

    // 이 이벤트에 도달했을 때 호출될 UnityEvent
    public UnityEvent OnDestroyEvent;
}

public class BeForDestroy : MonoBehaviour
{
    [Tooltip("이 게임 오브젝트가 파괴될 때 실행할 이벤트들을 등록하세요.")]
    public EventOnDestroy[] eventsToExecute; // 배열 이름 변경 (더 명확하게)

    private void OnDestroy()
    {
        Debug.Log($"<color=red>{gameObject.name} GameObject가 파괴될 때 OnDestroy() 호출됨.</color>");

        // 등록된 모든 이벤트들을 순회하며 호출
        foreach (var eventData in eventsToExecute)
        {
            if (eventData.OnDestroyEvent != null)
            {
                Debug.Log($"<color=orange>'{gameObject.name}'의 OnDestroy에서 '{eventData.eventName}' 이벤트 실행 중...</color>");
                eventData.OnDestroyEvent.Invoke(); // 등록된 모든 함수 호출
            }
            else
            {
                Debug.LogWarning($"'{gameObject.name}'의 OnDestroy에 등록된 '{eventData.eventName}' 이벤트가 null입니다.");
            }
        }
    }
}