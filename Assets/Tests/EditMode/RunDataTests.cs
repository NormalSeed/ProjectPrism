using NUnit.Framework;

public class RunDataTests
{
    [Test]
    public void RunData_StartsAtFloorOne()
    {
        var run = new RunData("Spirit1");
        Assert.AreEqual(1, run.Floor);
    }

    [Test]
    public void RunData_StartsAtRoomZero()
    {
        var run = new RunData("Spirit1");
        Assert.AreEqual(0, run.RoomIndex);
    }

    [Test]
    public void RunData_IsActiveAfterConstruction()
    {
        var run = new RunData("Spirit1");
        Assert.IsTrue(run.IsActive);
    }

    [Test]
    public void RunData_AdvanceFloor_IncrementsFloor()
    {
        var run = new RunData("Spirit1");
        run.AdvanceFloor();
        Assert.AreEqual(2, run.Floor);
    }

    [Test]
    public void RunData_AdvanceFloor_ResetsRoomIndex()
    {
        var run = new RunData("Spirit1");
        run.AdvanceRoom();
        run.AdvanceRoom();
        run.AdvanceFloor();
        Assert.AreEqual(0, run.RoomIndex);
    }

    [Test]
    public void RunData_AdvanceRoom_IncrementsRoomIndex()
    {
        var run = new RunData("Spirit1");
        run.AdvanceRoom();
        Assert.AreEqual(1, run.RoomIndex);
    }

    [Test]
    public void RunData_End_SetsIsActiveFalse()
    {
        var run = new RunData("Spirit1");
        run.End();
        Assert.IsFalse(run.IsActive);
    }

    [Test]
    public void RunData_AddScore_AccumulatesPoints()
    {
        var run = new RunData("Spirit1");
        run.AddScore(100);
        run.AddScore(50);
        Assert.AreEqual(150, run.Score);
    }

    [Test]
    public void RunManager_StartRun_SetsSelectedSpirit()
    {
        var manager = new RunManager();
        manager.StartRun("TestSpirit");
        Assert.AreEqual("TestSpirit", manager.CurrentRun.SelectedSpiritName);
    }

    [Test]
    public void RunManager_StartRun_SetsRunActive()
    {
        var manager = new RunManager();
        manager.StartRun("TestSpirit");
        Assert.IsTrue(manager.CurrentRun.IsActive);
    }

    [Test]
    public void RunManager_AdvanceFloor_DelegatesToRunData()
    {
        var manager = new RunManager();
        manager.StartRun("TestSpirit");
        manager.AdvanceFloor();
        Assert.AreEqual(2, manager.CurrentRun.Floor);
    }

    [Test]
    public void RunManager_AdvanceRoom_DelegatesToRunData()
    {
        var manager = new RunManager();
        manager.StartRun("TestSpirit");
        manager.AdvanceRoom();
        Assert.AreEqual(1, manager.CurrentRun.RoomIndex);
    }

    [Test]
    public void RunManager_EndRun_SetsRunInactive()
    {
        var manager = new RunManager();
        manager.StartRun("TestSpirit");
        manager.EndRun();
        Assert.IsFalse(manager.CurrentRun.IsActive);
    }
}
