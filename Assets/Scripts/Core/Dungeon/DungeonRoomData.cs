using System.Collections.Generic;

public class DungeonRoomData
{
    public string Id { get; }
    public RoomType Type { get; }
    public int Row { get; }
    public int Col { get; }
    public bool IsCleared { get; private set; }
    public List<string> ConnectedRoomIds { get; } = new List<string>();

    public DungeonRoomData(string id, RoomType type, int row, int col)
    {
        Id = id;
        Type = type;
        Row = row;
        Col = col;
    }

    public void Clear() => IsCleared = true;

    public void AddConnection(string roomId) => ConnectedRoomIds.Add(roomId);
}
