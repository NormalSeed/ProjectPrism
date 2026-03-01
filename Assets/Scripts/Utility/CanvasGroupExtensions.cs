using UnityEngine;

public static class CanvasGroupExtensions
{
    public static void SetUIActivation(this CanvasGroup cg, bool activated)
    {
        cg.alpha = activated ? 1f : 0f;
        cg.interactable = activated;
        cg.blocksRaycasts = activated;
    }
}
