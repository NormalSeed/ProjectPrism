using NUnit.Framework;
using System.Linq;

public class DungeonMapGeneratorTests
{
    private DungeonMapGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new DungeonMapGenerator(seed: 42);
    }

    [Test]
    public void GenerateFloor_ReturnsNonNullMap()
    {
        var map = _generator.GenerateFloor(1);
        Assert.IsNotNull(map);
    }

    [Test]
    public void GenerateFloor_MapHasBossRoom()
    {
        var map = _generator.GenerateFloor(1);
        Assert.IsTrue(map.Rooms.Values.Any(r => r.Type == RoomType.Boss));
    }

    [Test]
    public void GenerateFloor_MapHasAtLeastOneRestRoom()
    {
        var map = _generator.GenerateFloor(1);
        Assert.IsTrue(map.Rooms.Values.Any(r => r.Type == RoomType.Rest));
    }

    [Test]
    public void GenerateFloor_MapHasAtLeastOneTreasureRoom()
    {
        var map = _generator.GenerateFloor(1);
        Assert.IsTrue(map.Rooms.Values.Any(r => r.Type == RoomType.Treasure));
    }

    [Test]
    public void GenerateFloor_StartRoomIsBattleAtRowZero()
    {
        var map = _generator.GenerateFloor(1);
        var startRoom = map.GetCurrentRoom();
        Assert.AreEqual(RoomType.Battle, startRoom.Type);
        Assert.AreEqual(0, startRoom.Row);
    }

    [Test]
    public void GenerateFloor_BossRoomNotClearedInitially()
    {
        var map = _generator.GenerateFloor(1);
        var bossRoom = map.Rooms.Values.First(r => r.Type == RoomType.Boss);
        Assert.IsFalse(bossRoom.IsCleared);
    }

    [Test]
    public void GenerateFloor_StartRoomHasConnections()
    {
        var map = _generator.GenerateFloor(1);
        var startRoom = map.GetCurrentRoom();
        Assert.Greater(startRoom.ConnectedRoomIds.Count, 0);
    }

    [Test]
    public void DungeonMapManager_GenerateFloor_SetsCurrentMap()
    {
        var manager = new DungeonMapManager();
        manager.GenerateFloor(1);
        Assert.IsNotNull(manager.CurrentMap);
    }

    [Test]
    public void DungeonMapManager_IsFloorComplete_FalseInitially()
    {
        var manager = new DungeonMapManager();
        manager.GenerateFloor(1);
        Assert.IsFalse(manager.IsFloorComplete);
    }

    [Test]
    public void DungeonMapManager_ClearCurrentRoom_MarksRoomCleared()
    {
        var manager = new DungeonMapManager();
        manager.GenerateFloor(1);
        manager.ClearCurrentRoom();
        Assert.IsTrue(manager.CurrentMap.GetCurrentRoom().IsCleared);
    }
}
