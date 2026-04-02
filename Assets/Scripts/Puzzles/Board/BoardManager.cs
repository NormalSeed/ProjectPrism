using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BoardManager : MonoBehaviour, IBoardService
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform _parentRect;
    [SerializeField] private RectTransform _boardRect;
    private GridLayoutGroup _gridLayout;
    private int _gridSize = 5;

    [Header("Light Visualization")]
    [SerializeField] private Button _generateBoardButton;
    [SerializeField] private float _lightZOffset = -1f;

    [Header("Stage Config")]
    [SerializeField] private int _obstacleCount = 4;
    [SerializeField] private int _crystalCount = 2;

    [Header("Team & Inventory System")]
    [SerializeField] private List<SpiritData> _assignedSpirits = new List<SpiritData>();
    private Dictionary<PieceType, int> _remainingPieces = new Dictionary<PieceType, int>();
    private PieceType _selectedPieceType = PieceType.None;
    [SerializeField] private int _maxTeamSize = 3;
    [SerializeField] private List<Image> _spiritIcons = new List<Image>();

    [Header("Interaction UI")]
    [SerializeField] private Image _dragGhostIcon;
    [SerializeField] private TextMeshProUGUI _inventoryText;

    [SerializeField] private Tile[] _tiles;

    public event Action OnPieceCountChanged;

    private readonly BoardState _boardState = new BoardState();
    private StageGenerator _stageGenerator;
    private LightSimulator _lightSimulator;
    private bool _isGenerating = false;
    private CancellationToken _ct;

    private PuzzlePathFinder _pathFinder;
    private IGameService _gameService;

    [Inject]
    public void Construct(PuzzlePathFinder pathFinder, IGameService gameService)
    {
        _pathFinder = pathFinder;
        _gameService = gameService;
    }

    private void Awake()
    {
        if (_boardRect == null) _boardRect = GetComponent<RectTransform>();
        _gridLayout = GetComponent<GridLayoutGroup>();
        if (_parentRect == null && transform.parent != null)
            _parentRect = transform.parent.GetComponent<RectTransform>();

        _tiles = GetComponentsInChildren<Tile>();

        if (_dragGhostIcon != null) _dragGhostIcon.gameObject.SetActive(false);

        _ct = this.GetCancellationTokenOnDestroy();
    }

    private void Start()
    {
        InitializeAsync(_ct).Forget();
    }

    private async UniTaskVoid InitializeAsync(CancellationToken ct)
    {
        await UniTask.NextFrame(ct);

        _stageGenerator = new StageGenerator(_pathFinder, _gridSize, _obstacleCount, _crystalCount);
        _lightSimulator = new LightSimulator(_gridSize);

        UpdateBoardLayOut();

        if (_gameService != null)
        {
            _gameService.OnBoardChanged += (newStage) => CreateNewStageAsync().Forget();
        }

        if (_tiles != null && _tiles.Length > 0)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                int x = i % _gridSize;
                int y = i / _gridSize;
                _tiles[i].Init(x, y, OnTileClicked);
            }
            Debug.Log($"[Board] {_tiles.Length}개의 타일 상호작용 연결 완료.");
        }

        _generateBoardButton.onClick.AddListener(() =>
        {
            if (!_isGenerating) CreateNewStageAsync().Forget();
        });

        InitInventory();
    }

    // --- IBoardService ---

    public void SetTeam(List<SpiritData> team)
    {
        _assignedSpirits = team;
        InitInventory();
        UpdateSpiritIcons(team);
        Debug.Log($"[Board] 팀 편성 완료. 현재 출전 정령: {_assignedSpirits.Count}마리");
    }

    public void OnPieceButtonClicked(PieceType type)
    {
        if (_remainingPieces.ContainsKey(type) && _remainingPieces[type] > 0)
        {
            _selectedPieceType = (_selectedPieceType == type) ? PieceType.None : type;
            Debug.Log($"[Board] 기물 선택 상태: {_selectedPieceType}");
        }
        else
        {
            _selectedPieceType = PieceType.None;
        }
        UpdateInventoryUI();
    }

    public void OnPieceDropped(Vector2 screenPos, PieceType type)
    {
        foreach (var tile in _tiles)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(tile.GetComponent<RectTransform>(), screenPos))
            {
                TryPlacePiece(tile.X, tile.Y, type);
                break;
            }
        }
        SimulateCurrentLight();
    }

    public int GetRemainingPieceCount(PieceType type)
        => _remainingPieces.ContainsKey(type) ? _remainingPieces[type] : 0;

    public PieceType GetSelectedPieceType() => _selectedPieceType;

    public void OnTileClicked(int x, int y)
    {
        if (_isGenerating || _boardState.EmitterPos.x == -1) return;

        PieceType existing = _boardState.Grid[x, y];

        if (existing == PieceType.Mirror || existing == PieceType.Prism)
        {
            if (_boardState.Orientations[x, y] == 0)
            {
                _boardState.Orientations[x, y] = 1;
                Debug.Log($"[Board] ({x}, {y}) 기물 방향 전환: 1");
            }
            else
            {
                RemovePiece(x, y, existing);
                _boardState.Orientations[x, y] = 0;
            }
            _selectedPieceType = PieceType.None;
        }
        else if (existing == PieceType.None && _selectedPieceType != PieceType.None)
        {
            TryPlacePiece(x, y, _selectedPieceType);
            _selectedPieceType = PieceType.None;
        }

        SimulateCurrentLight();
    }

    public void TryPlacePiece(int x, int y, PieceType type)
    {
        if (_boardState.Grid[x, y] != PieceType.None) return;
        if (GetRemainingPieceCount(type) <= 0) return;

        _boardState.Grid[x, y] = type;
        _remainingPieces[type]--;

        UpdateInventoryUI();
        _lightSimulator.RefreshAllTiles(_boardState, _tiles);
    }

    private void RemovePiece(int x, int y, PieceType type)
    {
        _boardState.Grid[x, y] = PieceType.None;
        _remainingPieces[type]++;

        UpdateInventoryUI();
        _lightSimulator.RefreshAllTiles(_boardState, _tiles);
    }

    public void SimulateCurrentLight()
    {
        bool allSatisfied = _lightSimulator.Simulate(_boardState, _tiles);
        if (allSatisfied)
        {
            Debug.Log("<color=cyan>[Game] 모든 크리스탈 활성화 완료!</color>");
            _gameService?.CompleteBoard(_boardState.AllRequiredHits, _gameService.RemainingTime.Value);
        }
    }

    public async UniTaskVoid CreateNewStageAsync()
    {
        if (_isGenerating) return;
        if (!_gameService.isGameStarted) _gameService.isGameStarted = true;

        _isGenerating = true;
        if (_generateBoardButton != null) _generateBoardButton.interactable = false;

        int availableMirrors = 0;
        int availablePrisms = 0;

        foreach (var spirit in _assignedSpirits.Take(3).Where(s => s != null))
        {
            foreach (var p in spirit.startingPieces)
            {
                if (p.pieceType == PieceType.Mirror) availableMirrors += p.count;
                if (p.pieceType == PieceType.Prism) availablePrisms += p.count;
            }
        }

        if (availableMirrors + availablePrisms <= 0)
        {
            Debug.LogError("[Board] 사용할 수 있는 기물이 없습니다.");
            _isGenerating = false;
            if (_generateBoardButton != null) _generateBoardButton.interactable = true;
            return;
        }

        var result = await _stageGenerator.GenerateAsync(availableMirrors, availablePrisms, _ct);

        if (result.Success)
        {
            ApplyStageResult(result);
        }

        InitInventory();
        if (_generateBoardButton != null) _generateBoardButton.interactable = true;
        _isGenerating = false;
    }

    private void ApplyStageResult(StageGenerationResult result)
    {
        _boardState.Crystals = result.Crystals;
        _boardState.EmitterPos = result.EmitterPos;
        _boardState.EmitterDir = result.EmitterDir;
        _boardState.AllRequiredHits = 0;
        foreach (var c in _boardState.Crystals)
            _boardState.AllRequiredHits += c.RequiredHits;

        _boardState.ClearGrid();
        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                if (result.RawGrid[x, y] == 1) _boardState.Grid[x, y] = PieceType.Obstacle;
            }
        }
        _boardState.Grid[result.EmitterPos.x, result.EmitterPos.y] = PieceType.Emitter;
        foreach (var cry in _boardState.Crystals)
            _boardState.Grid[cry.Position.x, cry.Position.y] = PieceType.Crystal;

        SimulateCurrentLight();
    }

    // --- 인벤토리 ---

    private void InitInventory()
    {
        _remainingPieces.Clear();
        _remainingPieces[PieceType.Mirror] = 0;
        _remainingPieces[PieceType.Prism] = 0;

        foreach (var spirit in _assignedSpirits.Take(3))
        {
            if (spirit == null) continue;
            foreach (var p in spirit.startingPieces)
            {
                if (_remainingPieces.ContainsKey(p.pieceType))
                    _remainingPieces[p.pieceType] += p.count;
            }
        }

        _selectedPieceType = PieceType.None;
        UpdateInventoryUI();
    }

    public SpiritData GetPrimarySpirit()
        => _assignedSpirits.Count > 0 ? _assignedSpirits[0] : null;

    private void UpdateInventoryUI()
    {
        if (_inventoryText != null)
        {
            int m = GetRemainingPieceCount(PieceType.Mirror);
            int p = GetRemainingPieceCount(PieceType.Prism);
            _inventoryText.text = $"거울 : {m} | 프리즘 : {p}";
        }

        OnPieceCountChanged?.Invoke();
    }

    private void UpdateSpiritIcons(List<SpiritData> team)
    {
        foreach (Image icon in _spiritIcons)
        {
            if (icon == null) continue;
            Color c = icon.color;
            c.a = 0f;
            icon.color = c;
        }

        for (int i = 0; i < team.Count; i++)
        {
            if (i >= _spiritIcons.Count || _spiritIcons[i] == null || team[i] == null) continue;
            _spiritIcons[i].sprite = team[i].spiritIcon;
            Color c = _spiritIcons[i].color;
            c.a = 1f;
            _spiritIcons[i].color = c;
        }
    }

    // --- Drag & Drop ---

    public void SetDragGhost(bool active, Sprite sprite = null)
    {
        if (_dragGhostIcon == null) return;
        _dragGhostIcon.gameObject.SetActive(active);
        if (active && sprite != null) _dragGhostIcon.sprite = sprite;
    }

    public void UpdateDragGhostPos(Vector2 position)
    {
        if (_dragGhostIcon == null) return;
        _dragGhostIcon.transform.position = position;
    }

    // --- 타일 / 레이아웃 ---

    public Vector3 GetWorldPosition(int x, int y)
    {
        Tile tile = GetTile(x, y);
        return tile != null ? tile.transform.position : transform.position;
    }

    public Tile GetTile(int x, int y)
    {
        int index = (y * _gridSize) + x;
        if (index >= 0 && index < _tiles.Length) return _tiles[index];
        return null;
    }

    public void UpdateBoardLayOut()
    {
        if (_boardRect == null || _gridLayout == null) return;
        float boardSize = _boardRect.rect.width;
        float totalPadding = _gridLayout.padding.left + _gridLayout.padding.right;
        float totalSpacing = _gridLayout.spacing.x * (_gridSize - 1);
        float finalCellSize = (boardSize - totalPadding - totalSpacing) / _gridSize;
        _gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }
}
