using NUnit.Framework;

public class DungeonRoomDataTests
{
    [Test]
    public void DungeonRoomData_HasCorrectType()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        Assert.AreEqual(RoomType.Battle, room.Type);
    }

    [Test]
    public void DungeonRoomData_HasCorrectId()
    {
        var room = new DungeonRoomData("r42", RoomType.Rest, 1, 2);
        Assert.AreEqual("r42", room.Id);
    }

    [Test]
    public void DungeonRoomData_StartsNotCleared()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        Assert.IsFalse(room.IsCleared);
    }

    [Test]
    public void DungeonRoomData_Clear_SetsIsCleared()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        room.Clear();
        Assert.IsTrue(room.IsCleared);
    }

    [Test]
    public void DungeonRoomData_StartsWithNoConnections()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        Assert.AreEqual(0, room.ConnectedRoomIds.Count);
    }

    [Test]
    public void DungeonRoomData_AddConnection_IncreasesCount()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        room.AddConnection("r2");
        Assert.AreEqual(1, room.ConnectedRoomIds.Count);
    }

    [Test]
    public void DungeonRoomData_AddConnection_StoresId()
    {
        var room = new DungeonRoomData("r1", RoomType.Battle, 0, 0);
        room.AddConnection("r2");
        Assert.AreEqual("r2", room.ConnectedRoomIds[0]);
    }

    [Test]
    public void DungeonMapData_CurrentRoomId_MatchesStartRoom()
    {
        var start = new DungeonRoomData("r0", RoomType.Battle, 0, 0);
        var boss = new DungeonRoomData("r1", RoomType.Boss, 1, 0);
        var map = new DungeonMapData(new[] { start, boss }, "r0");
        Assert.AreEqual("r0", map.CurrentRoomId);
    }

    [Test]
    public void DungeonMapData_IsFloorComplete_FalseWhenBossNotCleared()
    {
        var boss = new DungeonRoomData("r1", RoomType.Boss, 1, 0);
        var map = new DungeonMapData(new[] { boss }, "r1");
        Assert.IsFalse(map.IsFloorComplete);
    }

    [Test]
    public void DungeonMapData_IsFloorComplete_TrueWhenBossCleared()
    {
        var boss = new DungeonRoomData("r1", RoomType.Boss, 1, 0);
        boss.Clear();
        var map = new DungeonMapData(new[] { boss }, "r1");
        Assert.IsTrue(map.IsFloorComplete);
    }
}
