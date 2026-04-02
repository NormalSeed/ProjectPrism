using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 보드의 모든 런타임 상태를 담는 데이터 컨테이너
/// </summary>
public class BoardState
{
    public int GridSize { get; } = 5;
    public PieceType[,] Grid { get; } = new PieceType[5, 5];
    public int[,] Orientations { get; } = new int[5, 5];
    public List<CrystalData> Crystals { get; set; } = new();
    public Vector2Int EmitterPos { get; set; } = new Vector2Int(-1, -1);
    public Vector2Int EmitterDir { get; set; } = Vector2Int.right;
    public int AllRequiredHits { get; set; } = 0;

    public void ClearGrid()
    {
        System.Array.Clear(Grid, 0, Grid.Length);
        System.Array.Clear(Orientations, 0, Orientations.Length);
    }
}
