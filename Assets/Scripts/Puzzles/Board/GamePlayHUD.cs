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

    private async UniTaskVoid InitializeAsync(CancellationToken ct)
    {
        await UniTask.NextFrame(ct);
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
        float size = _gameHUDRect.rect.width;
        float totalPadding = _gridLayout.padding.left + _gridLayout.padding.right;
        float totalSpacing = _gridLayout.spacing.x * (_gridSize - 1);
        float finalCellSize = (size - totalPadding - totalSpacing) / _gridSize;

        _gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
