using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayHUD : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform _parentRect;
    private RectTransform _gameHUDRect;
    private GridLayoutGroup _gridLayout;
    private int _gridSize = 5;
    private Vector2 _lastHUDRectSize;

    [Header("Spirit Info")]
    [SerializeField] private Image _spiritIcon;

    private IBoardService _boardService;

    [Inject]
    public void Construct(IBoardService boardService)
    {
        _boardService = boardService;
    }

    private void Awake()
    {
        _gameHUDRect = GetComponent<RectTransform>();
        _gridLayout = GetComponent<GridLayoutGroup>();

        if (_parentRect == null && transform.parent != null)
            _parentRect = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        InitializeAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void Update()
    {
        if (_gameHUDRect != null && _lastHUDRectSize != _gameHUDRect.rect.size)
            UpdateHUDLayout();
    }

    private async UniTaskVoid InitializeAsync(CancellationToken ct)
    {
        await UniTask.NextFrame(ct);
        Canvas.ForceUpdateCanvases();
        UpdateHUDLayout();

        if (_boardService != null)
        {
            _boardService.OnPieceCountChanged += RefreshSpiritDisplay;
            RefreshSpiritDisplay();
        }
    }

    private void OnDestroy()
    {
        if (_boardService != null)
            _boardService.OnPieceCountChanged -= RefreshSpiritDisplay;
    }

    private void RefreshSpiritDisplay()
    {
        var primarySpirit = _boardService.GetPrimarySpirit();
        if (primarySpirit != null && _spiritIcon != null)
        {
            _spiritIcon.sprite = primarySpirit.spiritIcon;
            _spiritIcon.gameObject.SetActive(true);
        }
        else if (_spiritIcon != null)
        {
            _spiritIcon.gameObject.SetActive(false);
        }
    }

    public void UpdateHUDLayout()
    {
        if (_gameHUDRect == null || _gridLayout == null) return;
        float hudWidth = _gameHUDRect.rect.width;
        if (hudWidth <= 0) return;

        _lastHUDRectSize = _gameHUDRect.rect.size;

        float totalHPadding = _gridLayout.padding.left + _gridLayout.padding.right;
        float totalHSpacing = _gridLayout.spacing.x * (_gridSize - 1);
        float cellSizeByWidth = (hudWidth - totalHPadding - totalHSpacing) / _gridSize;

        float cellSizeByHeight = cellSizeByWidth;
        float hudHeight = _gameHUDRect.rect.height;
        if (hudHeight > 0)
        {
            float totalVPadding = _gridLayout.padding.top + _gridLayout.padding.bottom;
            cellSizeByHeight = hudHeight - totalVPadding;
        }

        float finalCellSize = Mathf.Min(cellSizeByWidth, cellSizeByHeight);
        if (finalCellSize <= 0) return;
        _gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
