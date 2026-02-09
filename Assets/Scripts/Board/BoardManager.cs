using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BoardManager : MonoBehaviour, IBoardService
{
    // 보드 출력 비율 설정
    [Header("Layout Settings")]
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private float targetAspectRatio = 0.85f;

    private RectTransform boardRect;
    private GridLayoutGroup gridLayout;
    private int gridSize = 5;

    [Header("Light Visualization")]
    [SerializeField] private LineRenderer lightBeamRenderer;
    [SerializeField] private Button generateBoardButton;
    [SerializeField] private float lightZOffset = -1f;

    [Header("Stage Generation Config")]
    [SerializeField] private int obstacleCount = 4; // 장애물 개수
    [SerializeField] private int crystalCount = 2;  // 크리스탈 개수
    [SerializeField] private int maxPieces = 3;     // 사용할 수 있는 최대 기물 수

    [SerializeField] private Tile[] tiles;
    private PuzzlePathFinder pathFinder;

    // 게임 상태 데이터
    private PieceType[,] currentGrid = new PieceType[5, 5];
    private List<CrystalData> currentCrystals = new List<CrystalData>();
    private Vector2Int currentEmitterPos;
    private Vector2Int currentEmitterDir;

    private bool isGenerating = false;

    [Inject]
    public void Construct(PuzzlePathFinder _pathFinder)
    {
        pathFinder = _pathFinder;
    }

    private void Awake()
    {
        boardRect = GetComponent<RectTransform>();
        gridLayout = GetComponent<GridLayoutGroup>();
        if (parentRect == null) parentRect = transform.parent.GetComponent<RectTransform>();

        tiles = GetComponentsInChildren<Tile>();
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

    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        // Grid Layout Group의 순서대로 자식 오브젝트가 배치되어 있다고 가정
        int index = (y * gridSize) + x;
        if (index < transform.childCount)
        {
            return transform.GetChild(index).position;
        }

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
        isGenerating = true;
        generateBoardButton.interactable = false;   // 생성 중 버튼 비활성화

        lightBeamRenderer.positionCount = 0;    // 이전 선 지우기

        Debug.Log("스테이지 생성 중...");

        List<Vector2Int> validRoute = null;
        int[,] tempGenGrid = new int[gridSize, gridSize];
        List<CrystalData> tempCrystals = new List<CrystalData>();
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
            List<Vector2Int> shuffledPos = GetShuffledPositions();

            // 광원 위치 설정
            startPos = shuffledPos[0];
            shuffledPos.RemoveAt(0);
            startDir = GetValidStartDir(startPos);

            // 크리스탈 배치
            for (int i = 0; i < crystalCount; i++)
            {
                tempCrystals.Add(new CrystalData
                {
                    Position = shuffledPos[0],
                    RequiredHits = Random.Range(1, 3) // 1~2회 방문 필요
                });
                shuffledPos.RemoveAt(0);
            }

            // 장애물 배치
            for (int i = 0; i < obstacleCount; i++)
            {
                Vector2Int obsPos = shuffledPos[0];
                tempGenGrid[obsPos.x, obsPos.y] = 1; // 1은 장애물을 의미
                shuffledPos.RemoveAt(0);
            }

            // 비동기 검증 (A* 순회 탐색)
            validRoute = await pathFinder.FindFullRouteAsync(tempGenGrid, startPos, startDir, tempCrystals, maxPieces);

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
            currentCrystals = new List<CrystalData>(tempCrystals);
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

        generateBoardButton.interactable = true;
        isGenerating = false;
    }



    // 현재 보드에 배치된 기물(거울/프리즘)을 기반으로 빛을 계산하는 실제 게임 로직
    public void SimulateCurrentLight()
    {
        // 1. 모든 크리스탈의 현재 히트 초기화
        foreach (var cry in currentCrystals) cry.CurrentHits = 0;

        List<Vector2Int> path = new List<Vector2Int> { currentEmitterPos };
        Vector2Int currPos = currentEmitterPos;
        Vector2Int currDir = currentEmitterDir;

        // 2. 빛 추적 (최대 25칸까지만 순회)
        int safetyIndex = gridSize * gridSize;
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

            // TODO: 여기서 유저가 배치한 Mirror/Prism에 따른 방향 전환 로직 추가
            // if (type == PieceType.Mirror) currDir = ...
        }

        // 3. 결과 반영
        DrawLightPath(path);
        RefreshAllTiles();
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
            int rnd = Random.Range(0, i + 1);
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
