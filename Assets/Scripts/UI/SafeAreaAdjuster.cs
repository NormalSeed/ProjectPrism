using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdjuster : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Apply();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
            Apply();
    }

    private void Apply()
    {
        var safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;

        var anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        var anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}
