using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour, IBoardService
{
    [SerializeField] RectTransform parentRect;
    [SerializeField] float targetAspectRatio = 0.85f;

    RectTransform boardRect;
    GridLayoutGroup gridLayout;

    int gridSize = 5;

    void Awake()
    {
        boardRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();
        if (parentRect == null) parentRect = transform.parent.GetComponent<RectTransform>();
    }

    IEnumerator Start()
    {
        yield return null;
        UpdateBoardLayOut();
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        throw new System.NotImplementedException();
    }

    public void SetTile(int x, int y)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateBoardLayOut()
    {
        float containerWidth = parentRect.rect.width;
        float calculatedHeight = containerWidth / targetAspectRatio;

        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth);
        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, calculatedHeight);

        float boardSize = boardRect.rect.width;
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (gridSize - 1);
        float finalCellSize = (boardSize - totalPadding - totalSpacing) / gridSize;

        gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
