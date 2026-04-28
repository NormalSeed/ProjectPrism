using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIScaleInitializer
{
    private const string SettingsPath = "UIScaleSettings";
    private static UIScaleSettings _settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _settings = Resources.Load<UIScaleSettings>(SettingsPath);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.renderMode == RenderMode.WorldSpace) continue;
            if (!canvas.TryGetComponent<CanvasScaler>(out var scaler)) continue;
            ApplySettings(scaler);
        }
    }

    private static void ApplySettings(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        if (_settings != null)
        {
            scaler.referenceResolution = _settings.ReferenceResolution;
            scaler.screenMatchMode = _settings.ScreenMatchMode;
            scaler.matchWidthOrHeight = _settings.MatchWidthOrHeight;
        }
        else
        {
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}
