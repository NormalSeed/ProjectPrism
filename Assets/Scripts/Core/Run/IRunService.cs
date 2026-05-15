public interface IRunService
{
    RunData CurrentRun { get; }
    void StartRun(string spiritName);
    void AdvanceRoom();
    void AdvanceFloor();
    void EndRun();
}
