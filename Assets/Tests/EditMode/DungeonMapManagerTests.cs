using System;
using NUnit.Framework;

[TestFixture]
public class DungeonMapManagerTests
{
    private DungeonMapManager _manager;

    [SetUp]
    public void SetUp()
    {
        _manager = new DungeonMapManager();
        _manager.GenerateFloor(1);
    }

    [Test]
    public void EnterRoom_ReturnsTrue_WhenRoomIsConnected()
    {
        var nextId = _manager.CurrentMap.GetCurrentRoom().ConnectedRoomIds[0];
        Assert.IsTrue(_manager.EnterRoom(nextId));
    }

    [Test]
    public void EnterRoom_ReturnsFalse_WhenRoomIsNotConnected()
    {
        Assert.IsFalse(_manager.EnterRoom("nonexistent_room"));
    }

    [Test]
    public void EnterRoom_UpdatesCurrentRoom_WhenConnected()
    {
        var nextId = _manager.CurrentMap.GetCurrentRoom().ConnectedRoomIds[0];
        _manager.EnterRoom(nextId);
        Assert.AreEqual(nextId, _manager.CurrentMap.CurrentRoomId);
    }

    [Test]
    public void EnterRoom_DoesNotChangeRoom_WhenNotConnected()
    {
        var startId = _manager.CurrentMap.CurrentRoomId;
        _manager.EnterRoom("nonexistent_room");
        Assert.AreEqual(startId, _manager.CurrentMap.CurrentRoomId);
    }

    [Test]
    public void EnterRoom_FiresOnRoomEntered_WithCorrectType()
    {
        var nextId = _manager.CurrentMap.GetCurrentRoom().ConnectedRoomIds[0];
        var expectedType = _manager.CurrentMap.Rooms[nextId].Type;
        RoomType? firedType = null;
        _manager.OnRoomEntered += t => firedType = t;

        _manager.EnterRoom(nextId);

        Assert.IsNotNull(firedType);
        Assert.AreEqual(expectedType, firedType.Value);
    }

    [Test]
    public void EnterRoom_DoesNotFireEvent_WhenNotConnected()
    {
        bool eventFired = false;
        _manager.OnRoomEntered += _ => eventFired = true;
        _manager.EnterRoom("nonexistent_room");
        Assert.IsFalse(eventFired);
    }
}
