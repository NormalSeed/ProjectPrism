using System;

public interface IDungeonMapService
{
    event Action<RoomType> OnRoomEntered;
    DungeonMapData CurrentMap { get; }
    bool IsFloorComplete { get; }
    void GenerateFloor(int floor);
    bool EnterRoom(string roomId);
    void ClearCurrentRoom();
}
