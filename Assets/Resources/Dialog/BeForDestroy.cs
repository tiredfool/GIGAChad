using UnityEngine;
using UnityEngine.Events; // UnityEvent�� ����ϱ� ���� �ʿ�

// [System.Serializable] �Ӽ��� �߰��Ͽ� Inspector�� ����ǵ��� �մϴ�.
// class ��� struct�� ����Ͽ� ������ ����ϴ�.
[System.Serializable]
public struct EventOnDestroy
{
    // Inspector���� �� �̺�Ʈ�� �̸��� ������ �� �ֵ��� �ʵ� �߰� (���� ����)
    public string eventName;

    // �� �̺�Ʈ�� �������� �� ȣ��� UnityEvent
    public UnityEvent OnDestroyEvent;
}

public class BeForDestroy : MonoBehaviour
{
    [Tooltip("�� ���� ������Ʈ�� �ı��� �� ������ �̺�Ʈ���� ����ϼ���.")]
    public EventOnDestroy[] eventsToExecute; // �迭 �̸� ���� (�� ��Ȯ�ϰ�)

    private void OnDestroy()
    {
        Debug.Log($"<color=red>{gameObject.name} GameObject�� �ı��� �� OnDestroy() ȣ���.</color>");

        // ��ϵ� ��� �̺�Ʈ���� ��ȸ�ϸ� ȣ��
        foreach (var eventData in eventsToExecute)
        {
            if (eventData.OnDestroyEvent != null)
            {
                Debug.Log($"<color=orange>'{gameObject.name}'�� OnDestroy���� '{eventData.eventName}' �̺�Ʈ ���� ��...</color>");
                eventData.OnDestroyEvent.Invoke(); // ��ϵ� ��� �Լ� ȣ��
            }
            else
            {
                Debug.LogWarning($"'{gameObject.name}'�� OnDestroy�� ��ϵ� '{eventData.eventName}' �̺�Ʈ�� null�Դϴ�.");
            }
        }
    }
}