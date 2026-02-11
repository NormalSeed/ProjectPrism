using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayHUD : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform parentRect;

    private RectTransform gameHUDRect;
    private GridLayoutGroup gridLayout;
    private int gridSize = 5;

    private void Awake()
    {
        gameHUDRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();

        if (parentRect == null) parentRect = transform.parent.GetComponent<RectTransform>();
    }
    
    private IEnumerator Start()
    {
        yield return null;
        UpdateHUDLayout();
    }

    public void UpdateHUDLayout()
    {
        float size = gameHUDRect.rect.width;
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (gridSize - 1);
        float finalCellSize = (size - totalPadding - totalSpacing) / gridSize;

        gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
