using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour, IBoardService
{
    RectTransform boardRect;
    GridLayoutGroup gridLayout;

    int gridSize = 5;

    void Awake()
    {
        boardRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();
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
        float boardSize = boardRect.rect.width;
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (gridSize - 1);
        float finalCellSize = (boardSize - totalPadding - totalSpacing) / gridSize;

        gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
