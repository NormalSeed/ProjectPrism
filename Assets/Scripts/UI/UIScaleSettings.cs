using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Project Prism/UI Scale Settings", fileName = "UIScaleSettings")]
public class UIScaleSettings : ScriptableObject
{
    [SerializeField] private Vector2 _referenceResolution = new(1080, 1920);
    [SerializeField] private CanvasScaler.ScreenMatchMode _screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    [SerializeField, Range(0f, 1f)] private float _matchWidthOrHeight = 0.5f;

    public Vector2 ReferenceResolution => _referenceResolution;
    public CanvasScaler.ScreenMatchMode ScreenMatchMode => _screenMatchMode;
    public float MatchWidthOrHeight => _matchWidthOrHeight;
}
