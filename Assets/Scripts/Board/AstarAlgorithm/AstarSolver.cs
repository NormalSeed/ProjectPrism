using System.Collections.Generic;
using UnityEngine;

public class AstarSolver
{
    // 탐색 결과를 담을 클래스
    public class PathResult
    {
        public List<Vector2Int> Path;   // 실제 이동한 좌표들
        public int PieceCount;          // 사용된 기물 수
        public Vector2Int EndDir;       // 도착 시점의 방향 (다음 탐색의 시작 방향)

        public PathResult(List<Vector2Int> path, int count, Vector2Int endDir)
        {
            Path = path;
            PieceCount = count;
            EndDir = endDir;
        }
    }

    public PathResult GetPath(int[,] grid, Vector2Int start, Vector2Int startDir, Vector2Int goal)
    {
        List<PathNode> openList = new List<PathNode>();
        Dictionary<(Vector2Int, Vector2Int), int> closedList = new Dictionary<(Vector2Int, Vector2Int), int>();

        // 초기화: 현재 빛이 들어오고 있는 방향(startDir)을 기준으로 시작
        openList.Add(new PathNode(start, startDir, 0, GetHeuristic(start, goal)));

        while (openList.Count > 0)
        {
            openList.Sort();
            PathNode current = openList[0];
            openList.RemoveAt(0);

            // 목표 도달 시 경로 역추적 및 결과 반환
            if (current.Position == goal)
            {
                return new PathResult(ReconstructPath(current), current.GCost, current.Direction);
            }

            var stateKey = (current.Position, current.Direction);
            if (closedList.ContainsKey(stateKey) && closedList[stateKey] <= current.GCost)
                continue;

            closedList[stateKey] = current.GCost;

            foreach (var nextDir in GridDirections.All)
            {
                // 180도 역주행 방지
                if (nextDir == -current.Direction) continue;

                Vector2Int nextPos = current.Position + nextDir;

                if (!IsValid(nextPos, grid)) continue;

                // 대각선 이동 로직 핵심:
                // 방향이 이전과 완전히 같으면 비용 0 (직진)
                // 방향이 조금이라도 다르면 비용 1 (기물 사용: 거울 or 프리즘)
                int moveCost = (nextDir == current.Direction) ? 0 : 1;
                int newCost = current.GCost + moveCost;

                // 휴리스틱 계산 및 노드 추가
                openList.Add(new PathNode(nextPos, nextDir, newCost, GetHeuristic(nextPos, goal), current));
            }
        }

        return null; // 경로 없음
    }

    // 부모 노드를 추적해 List<Vector2Int> 경로 생성
    private List<Vector2Int> ReconstructPath(PathNode node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        // 목표와 x, y 좌표가 모두 다르면 최소 1번은 꺾어야 함을 의미
        // 대각선 이동이 가능하므로, dx와 dy가 같으면 0번 꺾어도 도달할 수 있음
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        // 현재 위치에서 목표까지 일직선(대각선 포함)으로 갈 수 없으면 비용 발생 예상
        // dx != 0, dy != 0 이면서 dx != dy 인 경우
        if (dx > 0 && dy > 0 && dx != dy) return 1;

        // 그 외 (같은 행, 같은 열, 또는 완벽한 대각선상에 위치)
        return 0;
    }

    bool IsValid(Vector2Int pos, int[,] grid)
    {
        return pos.x >= 0 && pos.x < 5 && pos.y >= 0 && pos.y < 5 && grid[pos.x, pos.y] != 1;
    }
}
