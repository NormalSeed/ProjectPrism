using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private LineRenderer lightBeamRenderer;
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

    [Header("Interaction UI")]
    [SerializeField] Image dragGhostIcon;
    [SerializeField] TextMeshProUGUI inventoryText;

    [SerializeField] private Tile[] tiles;
    private PuzzlePathFinder pathFinder;

    // 게임 상태 데이터
    private PieceType[,] currentGrid = new PieceType[5, 5];
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
            RemovePiece(x, y, existing);
            // 회수 시 선택 상태를 회수한 기물로 변경하거나 None으로 초기화 (기획에 따라 선택)
            selectedPieceType = PieceType.None;
        }
        // 2. 빈 칸이고 선택된 기물이 있는 경우 : 배치
        else if (existing == PieceType.None && selectedPieceType != PieceType.None)
        {
            TryPlacePiece(x, y, selectedPieceType);
            // 연속 배치를 위해 선택 상태 유지 가능 (여기서는 1회 배치 후 해제)
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

    public void DrawLightPath(List<Vector2Int> route)
    {
        // 경로가 없거나 비어있으면 선 삭제
        if (route == null || route.Count == 0)
        {
            lightBeamRenderer.positionCount = 0;
            return;
        }

        // LineRenderer가 그릴 점의 개수 설정
        lightBeamRenderer.positionCount = route.Count;

        // 경로의 각 그리드 좌표를 월드 좌표로 변환하여 선의 정점으로 설정
        for (int i = 0; i < route.Count; i++)
        {
            Vector3 worldPos = GetWorldPosition(route[i].x, route[i].y);

            // UI와 겹치지 않게 Z축을 살짝 앞으로 당겨줌
            worldPos.z = -1f;

            lightBeamRenderer.SetPosition(i, worldPos);
        }

        Debug.Log($"[Viz] 빛 경로 그리기 완료. 총 {route.Count}개의 지점 연결.");
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
        lightBeamRenderer.positionCount = 0;    // 이전 선 지우기

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
                originalHits.Add(hits);
                shuffledPos.Remove(cPos);

                // PuzzlePathFinder에게 이 자리는 "기물 설치 불가능"임을 알리기 위해 그리드에 마킹 (2 = Crystal)
                tempGenGrid[cPos.x, cPos.y] = 2;
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

            Debug.Log($"{attemptCount}번의 시도 끝에 맵을 찾았습니다.");

            // 시각화 업데이트
            ApplyStageToUI();

            Debug.Log($"{attemptCount}번의 시도 끝에 맵을 찾았습니다.");

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

        // 모든 크리스탈의 현재 히트 초기화
        foreach (var cry in currentCrystals) cry.CurrentHits = 0;

        List<Vector2Int> path = new List<Vector2Int> { currentEmitterPos };
        Vector2Int currPos = currentEmitterPos;
        Vector2Int currDir = currentEmitterDir;

        // 방향이 0이면 진행 불가
        if (currDir == Vector2Int.zero)
        {
            Debug.LogError("[BoardManager] 광원의 발사 방향이 (0,0)입니다. 설정을 확인하세요.");
            DrawLightPath(path);
            RefreshAllTiles();
            return;
        }

        // 빛 추적 (최대 25칸까지만 순회)
        int safetyIndex = gridSize * gridSize * 2;
        while (safetyIndex-- > 0)
        {
            Vector2Int nextPos = currPos + currDir;

            // 보드 밖으로 나가면 중단
            if (nextPos.x < 0 || nextPos.x >= gridSize || nextPos.y < 0 || nextPos.y >= gridSize)
                break;

            path.Add(nextPos);
            currPos = nextPos;

            // 크리스탈 통과 시 카운트 (이후 기물 배치 시 0으로 초기화 후 재계산됨)
            var crystal = currentCrystals.Find(c => c.Position == currPos);
            if (crystal != null) crystal.CurrentHits++;

            PieceType type = currentGrid[currPos.x, currPos.y];

            // 장애물에 막힘
            if (type == PieceType.Obstacle) break;

            // 유저가 배치한 Mirror/Prism에 따른 방향 전환 로직
            if (type == PieceType.Mirror)
            {
                // 거울 반사: 90도 회전 (대각선 거울)
                currDir = new Vector2Int(-currDir.y, -currDir.x);
            }
            else if (type == PieceType.Prism)
            {
                // 프리즘 굴절: 45도 굴절 (Cardinals to Diagonals)
                Vector2Int newDir = currDir + new Vector2Int(currDir.y == 0 ? 0 : currDir.x, currDir.x == 0 ? 0 : currDir.y);
                currDir = new Vector2Int(Mathf.Clamp(newDir.x, -1, 1), Mathf.Clamp(newDir.y, -1, 1));
            }

            // 굴절 후 방향이 0이 되면 중단
            if (currDir == Vector2Int.zero) break;
        }

        // 결과 반영
        DrawLightPath(path);
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
                Vector2Int? eDir = (new Vector2Int(x, y) == currentEmitterPos) ? (Vector2Int?)currentEmitterDir : null;

                tile.SetState(type, cryData, eDir);
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
