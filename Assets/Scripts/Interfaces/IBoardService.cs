using UnityEngine;

public interface IBoardService
{
    void UpdateBoardLayOut();
    Vector3 GetWorldPosition(int x, int y);
    void SetTile(int x, int y);
}
