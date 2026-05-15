using System;

public class DungeonMapManager : IDungeonMapService
{
    private readonly DungeonMapGenerator _generator;

    public event Action<RoomType> OnRoomEntered;
    public DungeonMapData CurrentMap { get; private set; }
    public bool IsFloorComplete => CurrentMap?.IsFloorComplete ?? false;

    public DungeonMapManager()
    {
        _generator = new DungeonMapGenerator(Environment.TickCount);
    }

    public void GenerateFloor(int floor)
    {
        CurrentMap = _generator.GenerateFloor(floor);
    }

    public bool EnterRoom(string roomId)
    {
        var current = CurrentMap?.GetCurrentRoom();
        if (current == null || !current.ConnectedRoomIds.Contains(roomId)) return false;
        CurrentMap.MoveToRoom(roomId);
        OnRoomEntered?.Invoke(CurrentMap.Rooms[roomId].Type);
        return true;
    }

    public void ClearCurrentRoom()
    {
        CurrentMap?.GetCurrentRoom().Clear();
    }
}
