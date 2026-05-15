public class RunManager : IRunService
{
    public RunData CurrentRun { get; private set; }

    public void StartRun(string spiritName)
    {
        CurrentRun = new RunData(spiritName);
    }

    public void AdvanceRoom()
    {
        CurrentRun?.AdvanceRoom();
    }

    public void AdvanceFloor()
    {
        CurrentRun?.AdvanceFloor();
    }

    public void EndRun()
    {
        CurrentRun?.End();
    }
}
