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
    [SerializeField] RectTransform parentRect;
    [SerializeField] float targetAspectRatio = 0.85f;

    RectTransform boardRect;
    GridLayoutGroup gridLayout;

    int gridSize = 5;

    [Header("Light Visualization")]
    [SerializeField] LineRenderer lightBeamRenderer;
    [SerializeField] Button generateBoardButton;

    [Header("Stage Generation Config")]
    [SerializeField] int obstacleCount = 4; // 장애물 개수
    [SerializeField] int crystalCount = 2;  // 크리스탈 개수
    [SerializeField] int maxPieces = 3;     // 사용할 수 있는 최대 기물 수

    PuzzlePathFinder pathFinder;

    [Inject]
    public void Construct(PuzzlePathFinder _pathFinder)
    {
        pathFinder = _pathFinder;
    }

    private bool isGenerating = false;

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
        generateBoardButton.onClick.AddListener(() => CreateNewStageAsync().Forget());
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        // Grid Layout Group의 순서대로 자식 오브젝트가 배치되어 있다고 가정
        int index = (y * gridSize) + x;
        if (index < transform.childCount)
        {
            return transform.GetChild(index).position;
        }

        Debug.LogWarning($"[BoardManager] 범위에서 벗어났습니다.");
        return transform.position;
    }

    public void SetTile(int x, int y)
    {
        // TODO: 해당 좌표의 타일 속성을 변경하거나 기물을 배치하는 시각적 연출
        Debug.Log($"[Board Manager] Tile Set: {x}, {y}");
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

        Debug.Log("스테이지 생성 중...");

        List<Vector2Int> validRoute = null;
        int[,] grid = new int[5, 5];
        List<CrystalData> crystals = new List<CrystalData>();
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int startDir = Vector2Int.right;

        int attemptCount = 0;

        // 풀 수 있는 맵이 나올 때까지 무한 반복
        while (validRoute == null)
        {
            attemptCount++;

            // 데이터 초기화
            System.Array.Clear(grid, 0, grid.Length);
            crystals.Clear();
            List<Vector2Int> shuffledPos = GetShuffledPositions();

            startPos = shuffledPos[0];
            shuffledPos.RemoveAt(0);
            startDir = GetValidStartDir(startPos);

            for (int i = 0; i < crystalCount; i++)
            {
                crystals.Add(new CrystalData
                {
                    Position = shuffledPos[0],
                    RequiredHits = Random.Range(1, 3) // 1~2회 방문 필요
                });
                shuffledPos.RemoveAt(0);
            }

            // 4. 장애물 배치
            for (int i = 0; i < obstacleCount; i++)
            {
                Vector2Int obsPos = shuffledPos[0];
                grid[obsPos.x, obsPos.y] = 1; // 1은 장애물을 의미
                shuffledPos.RemoveAt(0);
            }

            // 5. 비동기 검증 (A* 순회 탐색)
            validRoute = await pathFinder.FindFullRouteAsync(grid, startPos, startDir, crystals, maxPieces);

            if (attemptCount % 50 == 0) await UniTask.Yield();

            // 너무 많이 시도하면 잠시 쉬어줌
            if (attemptCount > 1000)
            {
                Debug.LogWarning("적절한 맵을 찾지 못해 생성을 중단합니다.");
                break;
            }
        }

        Debug.Log($"{attemptCount}번의 시도 끝에 맵을 찾았습니다.");

        // 시각적 배치
        DrawLightPath(validRoute);

        ApplyStageToUI(grid, crystals);

        isGenerating = false;
    }

    Vector2Int GetValidStartDir(Vector2Int pos)
    {
        if (pos.x == 0) return Vector2Int.right;
        if (pos.x == gridSize - 1) return Vector2Int.left;
        if (pos.y == 0) return Vector2Int.up;
        if (pos.y == gridSize - 1) return Vector2Int.down;
        return Vector2Int.right; // 중앙에 있을 경우 기본값
    }

    List<Vector2Int> GetShuffledPositions()
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

    void ApplyStageToUI(int[,] grid, List<CrystalData> crystals)
    {
        // 실제 UI 타일들을 활성화/비활성화 하거나 크리스탈 아이콘을 띄움
    }
}
