using System.Collections.Generic;
using System.Linq;

public class DungeonMapPresenter
{
    private readonly IDungeonMapService _service;

    public DungeonMapPresenter(IDungeonMapService service)
    {
        _service = service;
    }

    public DungeonRoomData GetCurrentRoom() => _service.CurrentMap.GetCurrentRoom();

    public List<DungeonRoomData> GetNextRoomChoices()
    {
        var current = GetCurrentRoom();
        return current.ConnectedRoomIds
            .Where(id => _service.CurrentMap.Rooms.ContainsKey(id))
            .Select(id => _service.CurrentMap.Rooms[id])
            .ToList();
    }

    public bool SelectRoom(string roomId) => _service.EnterRoom(roomId);

    public IEnumerable<DungeonRoomData> GetAllRooms() => _service.CurrentMap.Rooms.Values;
}
