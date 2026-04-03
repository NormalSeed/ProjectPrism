using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class StageGenerationResult
{
    public bool Success { get; set; }
    public List<CrystalData> Crystals { get; set; }
    public Vector2Int EmitterPos { get; set; }
    public Vector2Int EmitterDir { get; set; }
    public int[,] RawGrid { get; set; }
    public int AttemptCount { get; set; }
}

/// <summary>
/// 백트래킹 알고리즘을 이용해 풀 수 있는 퍼즐 스테이지를 생성하는 클래스
/// </summary>
public class StageGenerator
{
    private readonly PuzzlePathFinder _pathFinder;
    private readonly int _gridSize;
    private readonly int _obstacleCount;
    private readonly int _crystalCount;

    public StageGenerator(PuzzlePathFinder pathFinder, int gridSize, int obstacleCount, int crystalCount)
    {
        _pathFinder = pathFinder;
        _gridSize = gridSize;
        _obstacleCount = obstacleCount;
        _crystalCount = crystalCount;
    }

    public async UniTask<StageGenerationResult> GenerateAsync(int availableMirrors, int availablePrisms, CancellationToken ct = default)
    {
        int[,] tempGenGrid = new int[_gridSize, _gridSize];
        List<CrystalData> tempCrystals = new List<CrystalData>();
        List<int> originalHits = new List<int>();
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int startDir = Vector2Int.right;

        List<Vector2Int> validRoute = null;
        int attemptCount = 0;

        while (validRoute == null)
        {
            attemptCount++;

            System.Array.Clear(tempGenGrid, 0, tempGenGrid.Length);
            tempCrystals.Clear();
            originalHits.Clear();

            List<Vector2Int> shuffledPos = GetShuffledPositions();
            List<Vector2Int> edgePositions = GetShuffledEdgePositions();

            startPos = edgePositions[0];
            shuffledPos.Remove(startPos);
            startDir = GetValidStartDir(startPos);
            tempGenGrid[startPos.x, startPos.y] = 3;

            // 광원 바로 앞 칸에는 장애물/크리스탈 배치 불가
            Vector2Int nextToEmitter = startPos + startDir;
            shuffledPos.Remove(nextToEmitter);

            // 광원에서 직선으로 바로 맞는 위치는 크리스탈 배치 제외
            var nonDirectPosList = shuffledPos.Where(p => !IsDirectHit(startPos, startDir, p)).ToList();
            if (nonDirectPosList.Count < _crystalCount) continue;

            for (int i = 0; i < _crystalCount; i++)
            {
                Vector2Int cPos = nonDirectPosList[i];
                int hits = UnityEngine.Random.Range(1, 3);
                tempCrystals.Add(new CrystalData { Position = cPos, RequiredHits = hits });
                tempGenGrid[cPos.x, cPos.y] = 2;
                originalHits.Add(hits);
                shuffledPos.Remove(cPos);
            }

            for (int i = 0; i < _obstacleCount; i++)
            {
                Vector2Int obsPos = shuffledPos[0];
                tempGenGrid[obsPos.x, obsPos.y] = 1;
                shuffledPos.RemoveAt(0);
            }

            validRoute = await _pathFinder.FindFullRouteAsync(
                tempGenGrid, startPos, startDir, tempCrystals, availableMirrors, availablePrisms, ct);

            if (attemptCount % 50 == 0) await UniTask.Yield(ct);

            if (attemptCount > 2000)
            {
                Debug.LogError("[StageGenerator] 적절한 맵 생성 실패 (조건 완화 필요)");
                return new StageGenerationResult { Success = false, AttemptCount = attemptCount };
            }
        }

        Debug.Log($"[StageGenerator] {attemptCount}번의 시도 끝에 맵을 찾았습니다.");

        var finalCrystals = new List<CrystalData>();
        for (int i = 0; i < tempCrystals.Count; i++)
        {
            finalCrystals.Add(new CrystalData
            {
                Position = tempCrystals[i].Position,
                RequiredHits = originalHits[i],
                CurrentHits = 0
            });
        }

        return new StageGenerationResult
        {
            Success = true,
            Crystals = finalCrystals,
            EmitterPos = startPos,
            EmitterDir = startDir,
            RawGrid = tempGenGrid,
            AttemptCount = attemptCount,
        };
    }

    /// <summary>
    /// 광원 위치와 방향 기준으로 대상이 직선상에 있는지 확인
    /// </summary>
    private bool IsDirectHit(Vector2Int origin, Vector2Int dir, Vector2Int target)
    {
        Vector2Int diff = target - origin;

        if (dir.x != 0 && diff.y != 0) return false;
        if (dir.y != 0 && diff.x != 0) return false;
        if (dir.x > 0 && diff.x <= 0) return false;
        if (dir.x < 0 && diff.x >= 0) return false;
        if (dir.y > 0 && diff.y <= 0) return false;
        if (dir.y < 0 && diff.y >= 0) return false;

        return true;
    }

    private Vector2Int GetValidStartDir(Vector2Int pos)
    {
        if (pos.x == 0) return GridDirections.Right;
        if (pos.x == _gridSize - 1) return GridDirections.Left;
        if (pos.y == 0) return GridDirections.Down;
        if (pos.y == _gridSize - 1) return GridDirections.Up;
        return GridDirections.Right;
    }

    private List<Vector2Int> GetShuffledPositions()
    {
        var positions = new List<Vector2Int>();
        for (int y = 0; y < _gridSize; y++)
            for (int x = 0; x < _gridSize; x++)
                positions.Add(new Vector2Int(x, y));

        // Fisher-Yates 셔플
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            (positions[i], positions[rnd]) = (positions[rnd], positions[i]);
        }
        return positions;
    }

    private List<Vector2Int> GetShuffledEdgePositions()
    {
        var positions = new List<Vector2Int>();
        for (int x = 0; x < _gridSize; x++)
        {
            positions.Add(new Vector2Int(x, 0));
            positions.Add(new Vector2Int(x, _gridSize - 1));
        }
        for (int y = 1; y < _gridSize - 1; y++)
        {
            positions.Add(new Vector2Int(0, y));
            positions.Add(new Vector2Int(_gridSize - 1, y));
        }

        // Fisher-Yates 셔플
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            (positions[i], positions[rnd]) = (positions[rnd], positions[i]);
        }
        return positions;
    }
}
