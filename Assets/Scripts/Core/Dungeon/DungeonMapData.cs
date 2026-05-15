using System.Collections.Generic;
using System.Linq;

public class DungeonMapData
{
    public Dictionary<string, DungeonRoomData> Rooms { get; }
    public string CurrentRoomId { get; private set; }

    public bool IsFloorComplete => Rooms.Values.Any(r => r.Type == RoomType.Boss && r.IsCleared);

    public DungeonMapData(IEnumerable<DungeonRoomData> rooms, string startRoomId)
    {
        Rooms = rooms.ToDictionary(r => r.Id);
        CurrentRoomId = startRoomId;
    }

    public DungeonRoomData GetCurrentRoom() => Rooms[CurrentRoomId];

    public void MoveToRoom(string roomId) => CurrentRoomId = roomId;
}
