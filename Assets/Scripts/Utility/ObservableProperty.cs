using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class ObservableProperty<T>
{
    // 값이 바뀔 때마다 자동으로 알림을 보내주는 Observable Property
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            // _value가 null일 때 .Equals를 호출하면 에러가 나기 때문에 EqualityComparer<T>.Default.Equals를 사용해 비교
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            Notify();
        }
    }

    [System.NonSerialized]
    private UnityEvent<T> onValueChanged = new();
    public ObservableProperty(T value = default)
    {
        _value = value;
    }

    public void Subscribe(UnityAction<T> action)
    {
        onValueChanged.AddListener(action);
        action.Invoke(_value);
    }

    public void Unsubscribe(UnityAction<T> action)
    {
        onValueChanged.RemoveListener(action);
    }

    public void UnsubscribeAll()
    {
        onValueChanged.RemoveAllListeners();
    }

    private void Notify()
    {
        onValueChanged?.Invoke(Value);
    }
}