using System;
using System.Collections.Generic;
using System.Linq;

public class DungeonMapGenerator
{
    private readonly Random _random;
    private const int TotalRows = 5;
    private const int MinRoomsPerRow = 2;
    private const int MaxRoomsPerRow = 3;

    public DungeonMapGenerator(int seed = 0)
    {
        _random = new Random(seed);
    }

    public DungeonMapData GenerateFloor(int floor)
    {
        var rooms = new Dictionary<string, DungeonRoomData>();
        var rowRoomIds = new List<List<string>>();

        var startId = "r0_0";
        rooms[startId] = new DungeonRoomData(startId, RoomType.Battle, 0, 0);
        rowRoomIds.Add(new List<string> { startId });

        for (int row = 1; row < TotalRows - 1; row++)
        {
            int count = _random.Next(MinRoomsPerRow, MaxRoomsPerRow + 1);
            var rowIds = new List<string>();
            for (int col = 0; col < count; col++)
            {
                string id = $"r{row}_{col}";
                rooms[id] = new DungeonRoomData(id, GetRandomMidRowType(), row, col);
                rowIds.Add(id);
            }
            rowRoomIds.Add(rowIds);
        }

        EnsureRoomTypeExists(rooms, rowRoomIds, RoomType.Rest);
        EnsureRoomTypeExists(rooms, rowRoomIds, RoomType.Treasure);

        string bossId = $"r{TotalRows - 1}_0";
        rooms[bossId] = new DungeonRoomData(bossId, RoomType.Boss, TotalRows - 1, 0);
        rowRoomIds.Add(new List<string> { bossId });

        ConnectRows(rooms, rowRoomIds);

        return new DungeonMapData(rooms.Values, startId);
    }

    private RoomType GetRandomMidRowType()
    {
        var types = new[] { RoomType.Battle, RoomType.Battle, RoomType.Elite, RoomType.Rest, RoomType.Treasure };
        return types[_random.Next(types.Length)];
    }

    private void EnsureRoomTypeExists(
        Dictionary<string, DungeonRoomData> rooms,
        List<List<string>> rowRoomIds,
        RoomType targetType)
    {
        if (rooms.Values.Any(r => r.Type == targetType)) return;
        for (int row = 1; row < rowRoomIds.Count; row++)
        {
            foreach (var id in rowRoomIds[row])
            {
                if (rooms[id].Type == RoomType.Battle)
                {
                    var old = rooms[id];
                    rooms[id] = new DungeonRoomData(id, targetType, old.Row, old.Col);
                    return;
                }
            }
        }
    }

    private void ConnectRows(Dictionary<string, DungeonRoomData> rooms, List<List<string>> rowRoomIds)
    {
        for (int row = 0; row < rowRoomIds.Count - 1; row++)
        {
            var fromRow = rowRoomIds[row];
            var toRow = rowRoomIds[row + 1];
            foreach (var toId in toRow)
            {
                var fromId = fromRow[_random.Next(fromRow.Count)];
                rooms[fromId].AddConnection(toId);
            }
            foreach (var fromId in fromRow)
            {
                if (rooms[fromId].ConnectedRoomIds.Count == 0)
                {
                    var toId = toRow[_random.Next(toRow.Count)];
                    rooms[fromId].AddConnection(toId);
                }
            }
        }
    }
}
