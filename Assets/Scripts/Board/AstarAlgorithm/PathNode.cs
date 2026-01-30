using UnityEngine;

public class PathNode : System.IComparable<PathNode>
{
    public Vector2Int Position;             // 현재 타일 좌표
    public Vector2Int Direction;            // 노드에 진입 시의 방향
    public int GCost;                       // 시작부터 현재 노드까지 사용한 기물의 수 (방향 전환 횟수)
    public int HCost;                       // 목표까지의 예상 비용 (Heuristic)
    public int FCost => GCost + HCost;

    public PathNode Parent;

    public PathNode(Vector2Int position, Vector2Int dir, int gCost, int hCost, PathNode parent = null)
    {
        Position = position;
        Direction = dir;
        GCost = gCost;
        HCost = hCost;
        Parent = parent;
    }

    // 우선순위 큐에서 FCost가 낮은 순으로 정렬하기 위한 비교 메서드
    public int CompareTo(PathNode other)
    {
        int compare = FCost.CompareTo(other.FCost);
        if (compare == 0) compare = HCost.CompareTo(other.HCost);
        return compare;
    }
}
