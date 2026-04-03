using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[Serializable]
public class CrystalData
{
    public Vector2Int Position;
    public int RequiredHits;    // 빛이 통과해야 하는 횟수
    public int CurrentHits;  // 현재 빛이 통과한 횟수

    // 남은 횟수 계산 (0 미만으로 내려가지 않음)
    public int RemainingHits => Mathf.Max(0, RequiredHits - CurrentHits);

    // 남은 횟수가 0이면 만족
    public bool IsSatisfied => RemainingHits <= 0;
}

public class PuzzlePathFinder
{
    private int _gridSize = 5;

    public PuzzlePathFinder() { }

    /// <summary>
    /// 보드 생성 시 호출되는 비동기 탐색 메서드
    /// </summary>
    public async UniTask<List<Vector2Int>> FindFullRouteAsync(
        int[,] grid,
        Vector2Int start,
        Vector2Int startDir,
        List<CrystalData> crystals,
        int mirrorCount,
        int prismCount,
        CancellationToken ct = default)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            // 시뮬레이션을 위한 크리스탈 복사본 생성
            var targetCrystals = crystals.Select(c => new CrystalData
            {
                Position = c.Position,
                RequiredHits = c.RequiredHits,
                CurrentHits = 0
            }).ToList();

            List<Vector2Int> route = new List<Vector2Int>();

            // 백트래킹 탐색 시작
            if (Solve(grid, start, startDir, targetCrystals, mirrorCount, prismCount, 0, route))
            {
                return route;
            }
            return null;
        }, cancellationToken: ct);
    }

    /// <summary>
    /// 빛의 경로를 한 단계씩 시뮬레이션하며 기물 배치를 결정하는 재귀 함수 (Backtracking)
    /// </summary>
    private bool Solve(
        int[,] grid,
        Vector2Int currPos,
        Vector2Int currDir,
        List<CrystalData> crystals,
        int mirrors,
        int prisms,
        int depth,
        List<Vector2Int> path)
    {
        // 무한 루프 방지 (최대 이동 횟수)
        if (depth > 30) return false;

        path.Add(currPos);
        Vector2Int nextPos = currPos + currDir;

        // 1. 보드 경계 체크
        if (nextPos.x < 0 || nextPos.x >= _gridSize || nextPos.y < 0 || nextPos.y >= _gridSize)
        {
            // 보드 밖으로 나갔을 때 모든 크리스탈이 만족되었는지 확인
            return crystals.All(c => c.IsSatisfied);
        }

        // 2. 다음 칸의 상태 확인
        int tileType = grid[nextPos.x, nextPos.y];

        // [규칙] 장애물(1)에 부딪히면 종료. 이때 모든 목표 달성 여부 확인
        if (tileType == 1)
        {
            return crystals.All(c => c.IsSatisfied);
        }

        // 3. 크리스탈(2) 통과 시 히트 처리
        if (tileType == 2)
        {
            var cry = crystals.Find(c => c.Position == nextPos);
            if (cry != null) cry.CurrentHits++;
        }

        // 4. 기물 배치 및 이동 분기
        // CASE A: 빈 공간(0)인 경우 - 기물을 놓거나 그냥 지나가거나
        if (tileType == 0)
        {
            // 옵션 1: 그냥 지나가기 (직진)
            if (Solve(grid, nextPos, currDir, crystals, mirrors, prisms, depth + 1, path)) return true;

            // 옵션 2: 거울 배치 (90도 반사)
            if (mirrors > 0)
            {
                grid[nextPos.x, nextPos.y] = 4; // 기물 점유 기록 (백트래킹 시 재배치 방지)

                // [/] 방향 반사
                Vector2Int dirMirror1 = new Vector2Int(-currDir.y, -currDir.x);
                if (Solve(grid, nextPos, dirMirror1, crystals, mirrors - 1, prisms, depth + 1, path))
                {
                    grid[nextPos.x, nextPos.y] = 0;
                    return true;
                }

                // [\] 방향 반사
                Vector2Int dirMirror2 = new Vector2Int(currDir.y, currDir.x);
                if (Solve(grid, nextPos, dirMirror2, crystals, mirrors - 1, prisms, depth + 1, path))
                {
                    grid[nextPos.x, nextPos.y] = 0;
                    return true;
                }

                grid[nextPos.x, nextPos.y] = 0; // 복구
            }

            // 옵션 3: 프리즘 배치 (45도 굴절)
            if (prisms > 0)
            {
                grid[nextPos.x, nextPos.y] = 5; // 기물 점유 기록

                // 시계 반대방향 45도 굴절
                Vector2Int dirPrism1 = currDir + new Vector2Int(-currDir.y, currDir.x);
                dirPrism1 = new Vector2Int(Mathf.Clamp(dirPrism1.x, -1, 1), Mathf.Clamp(dirPrism1.y, -1, 1));
                if (Solve(grid, nextPos, dirPrism1, crystals, mirrors, prisms - 1, depth + 1, path))
                {
                    grid[nextPos.x, nextPos.y] = 0;
                    return true;
                }

                // 시계 방향 45도 굴절
                Vector2Int dirPrism2 = currDir + new Vector2Int(currDir.y, -currDir.x);
                dirPrism2 = new Vector2Int(Mathf.Clamp(dirPrism2.x, -1, 1), Mathf.Clamp(dirPrism2.y, -1, 1));
                if (Solve(grid, nextPos, dirPrism2, crystals, mirrors, prisms - 1, depth + 1, path))
                {
                    grid[nextPos.x, nextPos.y] = 0;
                    return true;
                }

                grid[nextPos.x, nextPos.y] = 0; // 복구
            }
        }
        else
        {
            // CASE B: 크리스탈(2)이나 광원(3)인 경우 - 기물 배치 불가, 빛은 통과
            if (Solve(grid, nextPos, currDir, crystals, mirrors, prisms, depth + 1, path)) return true;
        }

        // 5. 백트래킹: 상태 복구
        if (tileType == 2)
        {
            var cry = crystals.Find(c => c.Position == nextPos);
            if (cry != null) cry.CurrentHits--;
        }
        path.RemoveAt(path.Count - 1);
        return false;
    }
}
