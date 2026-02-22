using System.Collections.Generic;
using UnityEngine;

public interface IBoardService
{
    void UpdateBoardLayOut();
    Vector3 GetWorldPosition(int x, int y);
    Tile GetTile(int x, int y);
    void SimulateCurrentLight();

    void SetTeam(List<SpiritData> team);
}
