using System.Linq;
using UnityEngine;

/// <summary>
/// 보드 위의 빛 경로를 시뮬레이션하고 타일 시각을 업데이트하는 클래스
/// </summary>
public class LightSimulator
{
    private readonly int _gridSize;

    public LightSimulator(int gridSize)
    {
        _gridSize = gridSize;
    }

    /// <summary>
    /// 현재 보드 상태로 빛을 시뮬레이션한다.
    /// 모든 크리스탈이 만족되면 true를 반환한다.
    /// </summary>
    public bool Simulate(BoardState state, Tile[] tiles)
    {
        if (state.EmitterPos.x == -1) return false;

        foreach (var tile in tiles) tile.ClearLight();
        foreach (var cry in state.Crystals) cry.CurrentHits = 0;

        Vector2Int currPos = state.EmitterPos;
        Vector2Int currDir = state.EmitterDir;

        int safetyIndex = 100;
        while (safetyIndex-- > 0)
        {
            Tile currentTile = GetTile(tiles, currPos.x, currPos.y);
            Vector2Int inDir = currDir;

            PieceType type = state.Grid[currPos.x, currPos.y];
            int orient = state.Orientations[currPos.x, currPos.y];
            Vector2Int nextDir = currDir;

            if (type == PieceType.Mirror)
            {
                nextDir = (orient == 0)
                    ? new Vector2Int(-currDir.y, -currDir.x)
                    : new Vector2Int(currDir.y, currDir.x);
            }
            else if (type == PieceType.Prism)
            {
                Vector2Int offset = (orient == 0)
                    ? new Vector2Int(-currDir.y, currDir.x)
                    : new Vector2Int(currDir.y, -currDir.x);
                nextDir = currDir + offset;
                nextDir.x = Mathf.Clamp(nextDir.x, -1, 1);
                nextDir.y = Mathf.Clamp(nextDir.y, -1, 1);
            }
            else if (type == PieceType.Obstacle) break;

            currentTile?.SetLight(inDir, nextDir);

            currDir = nextDir;
            Vector2Int nextPos = currPos + currDir;

            if (nextPos.x < 0 || nextPos.x >= _gridSize || nextPos.y < 0 || nextPos.y >= _gridSize)
                break;

            currPos = nextPos;

            var crystal = state.Crystals.Find(c => c.Position == currPos);
            if (crystal != null) crystal.CurrentHits++;
        }

        RefreshAllTiles(state, tiles);
        return state.Crystals.Count > 0 && state.Crystals.All(c => c.IsSatisfied);
    }

    /// <summary>
    /// 현재 보드 상태를 기준으로 모든 타일의 시각 요소를 갱신한다.
    /// </summary>
    public void RefreshAllTiles(BoardState state, Tile[] tiles)
    {
        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                Tile tile = GetTile(tiles, x, y);
                if (tile == null) continue;

                PieceType type = state.Grid[x, y];
                CrystalData cryData = state.Crystals.Find(c => c.Position == new Vector2Int(x, y));
                int orientation = state.Orientations[x, y];
                Vector2Int? eDir = (new Vector2Int(x, y) == state.EmitterPos) ? (Vector2Int?)state.EmitterDir : null;

                tile.SetState(type, cryData, eDir, orientation);
            }
        }
    }

    private Tile GetTile(Tile[] tiles, int x, int y)
    {
        int index = (y * _gridSize) + x;
        if (index >= 0 && index < tiles.Length) return tiles[index];
        return null;
    }
}
