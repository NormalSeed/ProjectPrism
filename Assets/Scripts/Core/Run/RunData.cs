public class RunData
{
    public int Floor { get; private set; }
    public int RoomIndex { get; private set; }
    public string SelectedSpiritName { get; private set; }
    public bool IsActive { get; private set; }
    public int Score { get; private set; }

    public RunData(string spiritName)
    {
        SelectedSpiritName = spiritName;
        Floor = 1;
        RoomIndex = 0;
        IsActive = true;
        Score = 0;
    }

    public void AdvanceFloor()
    {
        Floor++;
        RoomIndex = 0;
    }

    public void AdvanceRoom()
    {
        RoomIndex++;
    }

    public void End()
    {
        IsActive = false;
    }

    public void AddScore(int points)
    {
        Score += points;
    }
}
