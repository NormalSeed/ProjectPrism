using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BoardManager : MonoBehaviour, IBoardService
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private RectTransform boardRect;
    private GridLayoutGroup gridLayout;
    private int gridSize = 5;

    [Header("Light Visualization")]
    [SerializeField] private Button generateBoardButton;
    [SerializeField] private float lightZOffset = -1f;

    [Header("Stage Config")]
    [SerializeField] private int obstacleCount = 4; // 장애물 개수
    [SerializeField] private int crystalCount = 2;  // 크리스탈 개수
    [SerializeField] private int maxPieces = 5;     // 사용할 수 있는 최대 기물 수

    [Header("Team & Inventory System")]
    [SerializeField] private List<SpiritData> assignedSpirits = new List<SpiritData>();
    private Dictionary<PieceType, int> remainingPieces = new Dictionary<PieceType, int>();
    private PieceType selectedPieceType = PieceType.None;
    [SerializeField] private int maxTeamSize = 3; // 지울 필요?
    [SerializeField] List<Image> spiritIcons = new List<Image>();

    [Header("Interaction UI")]
    [SerializeField] Image dragGhostIcon;
    [SerializeField] TextMeshProUGUI inventoryText;

    [SerializeField] private Tile[] tiles;
    private PuzzlePathFinder pathFinder;

    // 게임 상태 데이터
    private PieceType[,] currentGrid = new PieceType[5, 5];
    private int[,] currentOrientations = new int[5, 5]; // 0 또는 1

    private List<CrystalData> currentCrystals = new List<CrystalData>();
    private Vector2Int currentEmitterPos = new Vector2Int(-1, -1);
    private Vector2Int currentEmitterDir = Vector2Int.right;

    private bool isGenerating = false;

    public event Action OnPieceCountChanged;

    [Inject]
    public void Construct(PuzzlePathFinder _pathFinder)
    {
        pathFinder = _pathFinder;
    }

    private void Awake()
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();
        if (parentRect == null && transform.parent != null)
            parentRect = transform.parent.GetComponent<RectTransform>();

        tiles = GetComponentsInChildren<Tile>();

        if (dragGhostIcon != null) dragGhostIcon.gameObject.SetActive(false);
    }

    private IEnumerator Start()
    {
        yield return null;
        UpdateBoardLayOut();

        if (tiles != null && tiles.Length > 0)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                // 인덱스를 기반으로 x, y 좌표 계산 (5x5 그리드 기준)
                int x = i % gridSize;
                int y = i / gridSize;

                // 각 타일에 좌표와 클릭 시 실행될 메서드 전달
                tiles[i].Init(x, y, OnTileClicked);
            }
            Debug.Log($"[Board] {tiles.Length}개의 타일 상호작용 연결 완료.");
        }

        // 버튼에 리스너 등록 시 중복 등록 방지
        generateBoardButton.onClick.AddListener(() =>
        {
            if (!isGenerating) CreateNewStageAsync().Forget();
        });

        InitInventory();
        
    }

    // --- IBoardService 구현부 ---

    public void SetTeam(List<SpiritData> team)
    {
        assignedSpirits = team;
        InitInventory();
        UpdateSpiritIcons(team);
        Debug.Log($"[Board] 팀 편성 완료. 현재 출전 정령: {assignedSpirits.Count}마리");
    }

    public void OnPieceButtonClicked(PieceType type)
    {
        if (remainingPieces.ContainsKey(type) && remainingPieces[type] > 0)
        {
            // 선택 토글 (이미 선택된 것을 누르면 해제)
            selectedPieceType = (selectedPieceType == type) ? PieceType.None : type;
            Debug.Log($"[Board] 기물 선택 상태: {selectedPieceType}");
        }
        else
        {
            selectedPieceType = PieceType.None;
        }
        UpdateInventoryUI();
    }

    public void OnPieceDropped(Vector2 screenPos, PieceType type)
    {
        foreach (var tile in tiles)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(tile.GetComponent<RectTransform>(), screenPos))
            {
                // 드래그 드롭 시에는 해당 위치로 즉시 배치 시도
                TryPlacePiece(tile.X, tile.Y, type);
                break;
            }
        }
        SimulateCurrentLight();
    }

    public int GetRemainingPieceCount(PieceType type)
        => remainingPieces.ContainsKey(type) ? remainingPieces[type] : 0;

    public PieceType GetSelectedPieceType() => selectedPieceType;

    public void OnTileClicked(int x, int y)
    {
        if (isGenerating || currentEmitterPos.x == -1) return;

        PieceType existing = currentGrid[x, y];

        // 1. 이미 기물이 있는 경우 : 회수 (Mirror, Prism만 해당)
        if (existing == PieceType.Mirror || existing == PieceType.Prism)
        {
            // 방향 전환 시도 (0에서 1로)
            if (currentOrientations[x, y] == 0)
            {
                currentOrientations[x, y] = 1;
                Debug.Log($"[Board] ({x}, {y}) 기물 방향 전환: 1");
            }
            else
            {
                // 이미 방향이 1이면 다시 누를 때 회수
                RemovePiece(x, y, existing);
                // 방향 초기화
                currentOrientations[x, y] = 0;
            }
            selectedPieceType = PieceType.None;
        }
        // 2. 빈 칸이고 선택된 기물이 있는 경우 : 배치
        else if (existing == PieceType.None && selectedPieceType != PieceType.None)
        {
            TryPlacePiece(x, y, selectedPieceType);
            selectedPieceType = PieceType.None;
        }

        SimulateCurrentLight();
    }

    public void TryPlacePiece(int x, int y, PieceType type)
    {
        if (currentGrid[x, y] != PieceType.None) return;
        if (GetRemainingPieceCount(type) <= 0) return;

        currentGrid[x, y] = type;
        remainingPieces[type]--;

        UpdateInventoryUI();
        RefreshAllTiles();
    }

    private void RemovePiece(int x, int y, PieceType type)
    {
        currentGrid[x, y] = PieceType.None;
        remainingPieces[type]++;

        UpdateInventoryUI();
        RefreshAllTiles();
    }

    // --- 인벤토리 로직 ---

    private void InitInventory()
    {
        remainingPieces.Clear();
        remainingPieces[PieceType.Mirror] = 0;
        remainingPieces[PieceType.Prism] = 0;

        // 편성된 정령(최대 3마리)의 기물 정보를 합산
        var team = assignedSpirits.Take(3).ToList();
        foreach (var spirit in team)
        {
            if (spirit == null) continue;
            foreach (var p in spirit.startingPieces)
            {
                if (remainingPieces.ContainsKey(p.pieceType))
                    remainingPieces[p.pieceType] += p.count;
            }
        }

        selectedPieceType = PieceType.None;
        UpdateInventoryUI();
    }

    public SpiritData GetPrimarySpirit()
    {
        return assignedSpirits.Count > 0 ? assignedSpirits[0] : null;
    }

    private void UpdateInventoryUI()
    {
        // 텍스트 UI 업데이트
        if (inventoryText != null)
        {
            int m = GetRemainingPieceCount(PieceType.Mirror);
            int p = GetRemainingPieceCount(PieceType.Prism);
            inventoryText.text = $"거울 : {m} | 프리즘 : {p}";
        }

        OnPieceCountChanged?.Invoke();
    }

    private void UpdateSpiritIcons(List<SpiritData> team)
    {
        foreach (Image icon in spiritIcons)
        {
            if (icon == null) continue;

            Color c = icon.color;
            c.a = 0f;
            icon.color = c;
        }

        for (int i = 0; i < team.Count; i++)
        {
            if (i >= spiritIcons.Count) break;

            if (spiritIcons[i] == null || team[i] == null) continue;

            spiritIcons[i].sprite = team[i].spiritIcon;

            Color c = spiritIcons[i].color;
            c.a = 1f;
            spiritIcons[i].color = c;
        }
    }

    public void SetDragGhost(bool active, Sprite sprite = null)
    {
        if (dragGhostIcon == null) return;
        dragGhostIcon.gameObject.SetActive(active);
        if (active && sprite != null) dragGhostIcon.sprite = sprite;
    }

    public void UpdateDragGhostPos(Vector2 position)
    {
        if (dragGhostIcon == null) return;
        dragGhostIcon.transform.position = position;
    }


    public Vector3 GetWorldPosition(int x, int y)
    {
        Tile tile = GetTile(x, y);
        if (tile != null) return tile.transform.position;

        return transform.position;
    }

    public Tile GetTile(int x, int y)
    {
        int index = (y * gridSize) + x;
        if (index >= 0 && index < tiles.Length) return tiles[index];

        return null;
    }

    public void UpdateBoardLayOut()
    {
        if (boardRect == null || gridLayout == null) return;
        float boardSize = boardRect.rect.width;
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (gridSize - 1);
        float finalCellSize = (boardSize - totalPadding - totalSpacing) / gridSize;

        gridLayout.cellSize = new Vector2(finalCellSize, finalCellSize);
    }

    public async UniTaskVoid CreateNewStageAsync()
    {
        if (isGenerating) return;

        int totalAvailablePieces = GetTotalPieceCount();
        if (totalAvailablePieces <= 0)
        {
            Debug.LogError("[BoardManager] 배치된 정령의 기물이 0개입니다. 정령을 먼저 설정하세요.");
            return;
        }

        isGenerating = true;
        if (generateBoardButton != null) generateBoardButton.interactable = false;    // 생성 중 버튼 비활성화

        Debug.Log("스테이지 생성 중...");

        List<Vector2Int> validRoute = null;
        int[,] tempGenGrid = new int[gridSize, gridSize];

        List<CrystalData> tempCrystals = new List<CrystalData>();
        List<int> originalHits = new List<int>();

        Vector2Int startPos = Vector2Int.zero;
        Vector2Int startDir = Vector2Int.right;

        int attemptCount = 0;

        // 풀 수 있는 맵이 나올 때까지 무한 반복
        while (validRoute == null)
        {
            attemptCount++;

            // 데이터 초기화
            System.Array.Clear(tempGenGrid, 0, tempGenGrid.Length);
            tempCrystals.Clear();
            originalHits.Clear();

            List<Vector2Int> shuffledPos = GetShuffledPositions();

            // 광원 위치 설정
            startPos = shuffledPos[0];
            shuffledPos.RemoveAt(0);
            startDir = GetValidStartDir(startPos);
            tempGenGrid[startPos.x, startPos.y] = 3;

            // shuffledPos에서 광원 앞 칸을 찾아 제거하여 장애물이나 크리스탈이 놓이지 않게 함
            Vector2Int nextToEmitterPos = startPos + startDir;
            shuffledPos.Remove(nextToEmitterPos);

            // 크리스탈 배치
            var nonDirectPosList = shuffledPos.Where(p => !IsDirectHit(startPos, startDir, p)).ToList();

            if (nonDirectPosList.Count < crystalCount) continue;

            for (int i = 0; i < crystalCount; i++)
            {
                Vector2Int cPos = nonDirectPosList[i];
                int hits = UnityEngine.Random.Range(1, 3);
                tempCrystals.Add(new CrystalData { Position = cPos, RequiredHits = hits });
                tempGenGrid[cPos.x, cPos.y] = 2;
                originalHits.Add(hits);
                shuffledPos.Remove(cPos);
            }

            // 장애물 배치
            for (int i = 0; i < obstacleCount; i++)
            {
                Vector2Int obsPos = shuffledPos[0];
                tempGenGrid[obsPos.x, obsPos.y] = 1; // 1은 장애물을 의미
                shuffledPos.RemoveAt(0);
            }

            // 비동기 검증 (A* 순회 탐색)
            validRoute = await pathFinder.FindFullRouteAsync(tempGenGrid, startPos, startDir, tempCrystals, totalAvailablePieces);

            if (attemptCount % 50 == 0) await UniTask.Yield();

            if (attemptCount > 2000)
            {
                Debug.LogError("적절한 맵 생성 실패 (조건 완화 필요)");
                break;
            }
        }

        Debug.Log($"{attemptCount}번의 시도 끝에 맵을 찾았습니다.");

        if(validRoute != null)
        {
            // 성공한 데이터를 실제 게임 데이터로 저장
            currentCrystals = new List<CrystalData>();
            for (int i = 0; i < tempCrystals.Count; i++)
            {
                currentCrystals.Add(new CrystalData
                {
                    Position = tempCrystals[i].Position,
                    RequiredHits = originalHits[i],
                    CurrentHits = 0
                });
            }

            currentEmitterPos = startPos;
            currentEmitterDir = startDir;

            // PieceType 그리드 구성
            System.Array.Clear(currentGrid, 0, currentGrid.Length);
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (tempGenGrid[x, y] == 1) currentGrid[x, y] = PieceType.Obstacle;
                }
            }
            currentGrid[startPos.x, startPos.y] = PieceType.Emitter;
            foreach (var cry in currentCrystals)
            {
                currentGrid[cry.Position.x, cry.Position.y] = PieceType.Crystal;
            }

            // 시각화 업데이트
            ApplyStageToUI();
            SimulateCurrentLight();
        }

        InitInventory();
        if (generateBoardButton != null) generateBoardButton.interactable = true;
        isGenerating = false;
    }

    // 광원의 위치와 방향을 기준으로 대상(target)이 직선상에 있는지 확인
    private bool IsDirectHit(Vector2Int origin, Vector2Int dir, Vector2Int target)
    {
        Vector2Int diff = target - origin;

        // 방향 벡터가 X축일 때 Y가 같아야 하고, Y축일 때 X가 같아야 함
        if (dir.x != 0 && diff.y != 0) return false;
        if (dir.y != 0 && diff.x != 0) return false;

        // 발사 방향과 반대방향에 있는지도 체크
        if (dir.x > 0 && diff.x <= 0) return false;
        if (dir.x < 0 && diff.x >= 0) return false;
        if (dir.y > 0 && diff.y <= 0) return false;
        if (dir.y < 0 && diff.y >= 0) return false;

        return true;
    }

    // 현재 보드에 배치된 기물(거울/프리즘)을 기반으로 빛을 계산하는 실제 게임 로직
    public void SimulateCurrentLight()
    {
        if (currentEmitterPos.x == -1) return;

        foreach (var tile in tiles) tile.ClearLight();
        foreach (var cry in currentCrystals) cry.CurrentHits = 0;

        Vector2Int currPos = currentEmitterPos;
        Vector2Int currDir = currentEmitterDir;

        int safetyIndex = 100;
        while (safetyIndex-- > 0)
        {
            Tile currentTile = GetTile(currPos.x, currPos.y);
            Vector2Int inDir = currDir; // 이 타일로 들어온 방향

            // 1. 현재 타일의 기물에 따른 방향 전환 계산
            PieceType type = currentGrid[currPos.x, currPos.y];
            int orient = currentOrientations[currPos.x, currPos.y];

            Vector2Int nextDir = currDir; // 나갈 방향 (기본은 직진)

            if (type == PieceType.Mirror)
            {
                nextDir = (orient == 0) ? new Vector2Int(-currDir.y, -currDir.x) : new Vector2Int(currDir.y, currDir.x);
            }
            else if (type == PieceType.Prism)
            {
                Vector2Int rotationOffset = (orient == 0) ? new Vector2Int(-currDir.y, currDir.x) : new Vector2Int(currDir.y, -currDir.x);
                nextDir = currDir + rotationOffset;
                nextDir.x = Mathf.Clamp(nextDir.x, -1, 1);
                nextDir.y = Mathf.Clamp(nextDir.y, -1, 1);
            }
            else if (type == PieceType.Obstacle) break;

            // 2. 현재 타일에 빛 이미지 설정 (들어온 방향, 나갈 방향 전달)
            if (currentTile != null)
            {
                currentTile.SetLight(inDir, nextDir);
            }

            // 3. 다음 좌표로 이동
            currDir = nextDir;
            Vector2Int nextPos = currPos + currDir;

            if (nextPos.x < 0 || nextPos.x >= gridSize || nextPos.y < 0 || nextPos.y >= gridSize)
                break;

            currPos = nextPos;

            // 크리스탈 충돌 체크
            var crystal = currentCrystals.Find(c => c.Position == currPos);
            if (crystal != null) crystal.CurrentHits++;
        }

        RefreshAllTiles();
        CheckWinCondition();
    }

    private int GetTotalPieceCount()
    {
        int total = assignedSpirits.Take(maxTeamSize).Where(s => s != null).Sum(s => s.GetTotalPieceCount());
        return total;
    }

    private void CheckWinCondition()
    {
        if (currentCrystals.Count > 0 && currentCrystals.All(c => c.IsSatisfied))
            Debug.Log("<color=cyan>[Game] 스테이지 클리어!</color>");
    }

    private void RefreshAllTiles()
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Tile tile = GetTile(x, y);
                if (tile == null) continue;

                PieceType type = currentGrid[x, y];
                CrystalData cryData = currentCrystals.Find(c => c.Position == new Vector2Int(x, y));
                int orientation = currentOrientations[x, y];
                Vector2Int? eDir = (new Vector2Int(x, y) == currentEmitterPos) ? (Vector2Int?)currentEmitterDir : null;

                tile.SetState(type, cryData, eDir, orientation);
            }
        }
    }

    private Vector2Int GetValidStartDir(Vector2Int pos)
    {
        if (pos.x == 0) return GridDirections.Right;
        if (pos.x == gridSize - 1) return GridDirections.Left;
        if (pos.y == 0) return GridDirections.Down;
        if (pos.y == gridSize - 1) return GridDirections.Up;
        return GridDirections.Right; // 중앙에 있을 경우 기본값
    }

    private List<Vector2Int> GetShuffledPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }

        // Fisher-Yates 셔플 알고리즘
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            Vector2Int temp = positions[i];
            positions[i] = positions[rnd];
            positions[rnd] = temp;
        }
        return positions;
    }

    private void ApplyStageToUI()
    {
        // 모든 타일 초기화
        foreach (var tile in tiles) tile.ResetTile();
        RefreshAllTiles();
    }
}
