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
    AstarSolver solver;

    public PuzzlePathFinder(AstarSolver _solver)
    {
        solver = _solver;
    }

    public async UniTask<List<Vector2Int>> FindFullRouteAsync(
        int[,] grid, Vector2Int start, Vector2Int startDir,
        List<CrystalData> crystals, int maxPieces,
        CancellationToken ct = default)
    {
        // 계산 로직을 백그라운드 스레드로
        return await UniTask.RunOnThreadPool(() =>
        {
            // 기존에 작성했던 동기 메서드를 여기서 호출
            return FindFullRoute(grid, start, startDir, crystals, maxPieces);
        }, cancellationToken: ct);
    }

    public List<Vector2Int> FindFullRoute(int[,] grid, Vector2Int start, Vector2Int startDir, List<CrystalData> crystals, int maxPieces)
    {
        // 모든 크리스탈이 요구하는 총 방문 횟수의 합
        int totalRequiredHits = 0;
        foreach (var c in crystals)
        {
            totalRequiredHits += c.RequiredHits;
        }

        // 결과 경로를 담을 리스트
        List<Vector2Int> currentRoute = new List<Vector2Int>();
        currentRoute.Add(start);

        if (SolveRecursive(grid, start, startDir, crystals,
                            totalRequiredHits, 0, maxPieces, currentRoute))
        {
            return currentRoute;
        }

        return null; // 해결 불가능한 배치
    }

    bool SolveRecursive(int[,] grid, Vector2Int currentPos, Vector2Int currentDir,
                                List<CrystalData> crystals, int remainingHits,
                                int usedPieces, int maxPieces, List<Vector2Int> route)
    {
        // 모든 크리스탈을 정해진 횟수만큼 방문했으면 성공
        if (remainingHits == 0) return true;

        // 다음 방문할 크리스탈 후보 탐색
        foreach (var crystal in crystals)
        {
            if (crystal.RequiredHits <= 0) continue;

            // 현재 위치에서 타겟 크리스탈까지의 경로 탐색
            var segment = solver.GetPath(grid, currentPos, currentDir, crystal.Position);

            if (segment != null)
            {
                int newTotalPieces = usedPieces + segment.PieceCount;

                // 기물 제한 체크
                if (newTotalPieces <= maxPieces)
                {
                    // 경로 기록 및 크리스탈 카운트 감소
                    // segment.Path[0]은 현재 위치와 중복되므로 Skip(1)하여 추가
                    var addedNodes = segment.Path.Skip(1).ToList();
                    route.AddRange(addedNodes);
                    crystal.RequiredHits--;

                    if (SolveRecursive(grid, crystal.Position, segment.EndDir, crystals,
                                       remainingHits - 1, newTotalPieces, maxPieces, route))
                    {
                        return true;
                    }

                    // 탐색 실패 시 상태 되돌리기
                    crystal.RequiredHits++;
                    route.RemoveRange(route.Count - addedNodes.Count, addedNodes.Count);
                }
            }
        }

        return false;
    }
}
