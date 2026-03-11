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
            if (_value.Equals(value)) return;
            _value = value;
            Notify();
        }
    }

    private UnityEvent<T> onValueChanged = new();
    public ObservableProperty(T value = default)
    {
        _value = value;
    }

    public void Subscribe(UnityAction<T> action)
    {
        onValueChanged.AddListener(action);
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