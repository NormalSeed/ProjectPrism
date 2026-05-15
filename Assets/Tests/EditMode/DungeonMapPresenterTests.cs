using System.Linq;
using NUnit.Framework;

[TestFixture]
public class DungeonMapPresenterTests
{
    private class StubDungeonMapService : IDungeonMapService
    {
        public event System.Action<RoomType> OnRoomEntered;
        public DungeonMapData CurrentMap { get; set; }
        public bool IsFloorComplete => CurrentMap?.IsFloorComplete ?? false;
        public void GenerateFloor(int floor) { }
        public bool EnterRoom(string roomId)
        {
            var current = CurrentMap?.GetCurrentRoom();
            if (current == null || !current.ConnectedRoomIds.Contains(roomId)) return false;
            CurrentMap.MoveToRoom(roomId);
            OnRoomEntered?.Invoke(CurrentMap.Rooms[roomId].Type);
            return true;
        }
        public void ClearCurrentRoom() => CurrentMap?.GetCurrentRoom().Clear();
    }

    private static DungeonMapData BuildMap(params (string id, RoomType type, int row, int col, string[] connections)[] rooms)
    {
        var list = rooms.Select(r =>
        {
            var room = new DungeonRoomData(r.id, r.type, r.row, r.col);
            foreach (var c in r.connections) room.AddConnection(c);
            return room;
        }).ToList();
        return new DungeonMapData(list, rooms[0].id);
    }

    [Test]
    public void GetCurrentRoom_ReturnsStartRoom()
    {
        var map = BuildMap(("r0", RoomType.Battle, 0, 0, new string[0]));
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.AreEqual("r0", presenter.GetCurrentRoom().Id);
    }

    [Test]
    public void GetNextRoomChoices_ReturnsConnectedRooms()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1", "r2" }),
            ("r1", RoomType.Battle, 1, 0, new string[0]),
            ("r2", RoomType.Treasure, 1, 1, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.AreEqual(2, presenter.GetNextRoomChoices().Count);
    }

    [Test]
    public void GetNextRoomChoices_ReturnsEmpty_WhenNoConnections()
    {
        var map = BuildMap(("r0", RoomType.Battle, 0, 0, new string[0]));
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.IsEmpty(presenter.GetNextRoomChoices());
    }

    [Test]
    public void SelectRoom_ReturnsTrue_WhenRoomIsConnected()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1" }),
            ("r1", RoomType.Battle, 1, 0, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.IsTrue(presenter.SelectRoom("r1"));
    }

    [Test]
    public void SelectRoom_ReturnsFalse_WhenRoomIsNotConnected()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new string[0]),
            ("r1", RoomType.Battle, 1, 0, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.IsFalse(presenter.SelectRoom("r1"));
    }

    [Test]
    public void SelectRoom_MovesToRoom_WhenValid()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1" }),
            ("r1", RoomType.Battle, 1, 0, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        presenter.SelectRoom("r1");
        Assert.AreEqual("r1", presenter.GetCurrentRoom().Id);
    }

    [Test]
    public void SelectRoom_DoesNotMove_WhenRoomNotConnected()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new string[0]),
            ("r1", RoomType.Battle, 1, 0, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        presenter.SelectRoom("r1");
        Assert.AreEqual("r0", presenter.GetCurrentRoom().Id);
    }

    [Test]
    public void GetAllRooms_ReturnsAllRoomsInMap()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1", "r2" }),
            ("r1", RoomType.Battle, 1, 0, new string[0]),
            ("r2", RoomType.Treasure, 1, 1, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.AreEqual(3, presenter.GetAllRooms().Count());
    }

    [Test]
    public void GetCurrentRoom_AfterMapGeneration_IsRow0Room()
    {
        var service = new DungeonMapManager();
        service.GenerateFloor(1);
        var presenter = new DungeonMapPresenter(service);
        Assert.AreEqual(0, presenter.GetCurrentRoom().Row);
    }

    [Test]
    public void GetNextRoomChoices_ReflectsNewPosition_AfterMove()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1" }),
            ("r1", RoomType.Battle, 1, 0, new[] { "r2" }),
            ("r2", RoomType.Boss, 2, 0, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        presenter.SelectRoom("r1");
        var choices = presenter.GetNextRoomChoices();
        Assert.AreEqual(1, choices.Count);
        Assert.AreEqual("r2", choices[0].Id);
    }

    [Test]
    public void SelectRoom_MovesToCorrectRoom_WhenMultipleChoices()
    {
        var map = BuildMap(
            ("r0", RoomType.Battle, 0, 0, new[] { "r1", "r2" }),
            ("r1", RoomType.Rest, 1, 0, new string[0]),
            ("r2", RoomType.Treasure, 1, 1, new string[0])
        );
        var presenter = new DungeonMapPresenter(new StubDungeonMapService { CurrentMap = map });
        Assert.IsTrue(presenter.SelectRoom("r2"));
        Assert.AreEqual("r2", presenter.GetCurrentRoom().Id);
    }
}
