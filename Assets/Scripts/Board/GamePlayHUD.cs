using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayHUD : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform parentRect;
    private RectTransform gameHUDRect;
    private GridLayoutGroup gridLayout;
    private int gridSize = 5;

    [Header("Spirit Info")]
    [SerializeField] private Image spiritIcon; // 인스펙터에서 HUD의 정령 아이콘 Image 연결

    private IBoardService boardService;

    [Inject]
    public void Construct(IBoardService _boardService)
    {
        boardService = _boardService;
    }

    private void Awake()
    {
        gameHUDRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();

        if (parentRect == null && transform.parent != null)
            parentRect = transform.parent.GetComponent<RectTransform>();
    }

    private IEnumerator Start()
    {
        yield return null;
        UpdateHUDLayout();

        if (boardService != null)
        {
            // 보드 데이터(정령 포함)가 변경될 때마다 아이콘 갱신
            boardService.OnPieceCountChanged += RefreshSpiritDisplay;
            RefreshSpiritDisplay();
        }
    }

    private void OnDestroy()
    {
        if (boardService != null)
            boardService.OnPieceCountChanged -= RefreshSpiritDisplay;
    }

    private void RefreshSpiritDisplay()
    {
        var primarySpirit = boardService.GetPrimarySpirit();
        if (primarySpirit != null && spiritIcon != null)
        {
            spiritIcon.sprite = primarySpirit.spiritIcon;
            spiritIcon.gameObject.SetActive(true);
        }
        else if (spiritIcon != null)
        {
            spiritIcon.gameObject.SetActive(false);
        }
    }

    public void UpdateHUDLayout()
    {
        if (gameHUDRect == null || gridLayout == null) return;
        float size = gameHUDRect.rect.width;
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (gridSize - 1);
        float finalCellSize = (size - totalPadding - totalSpacing) / gridSize;

        gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
