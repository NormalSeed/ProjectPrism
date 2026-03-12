using System;

public interface IGameService
{
    // 스테이지가 변경되거나 클리어되었을 때 알릴 이벤트
    event Action<int> OnBoardChanged;
    event Action OnBoardClear;
    event Action<int> OnDamageCalculated;

    public ObservableProperty<int> StageLevel { get; }
    public ObservableProperty<float> remainingTime { get; }
    public ObservableProperty<float> boardDuration { get; }

    public bool isGameStarted { get; set; }

    // 스테이지 관리
    void CompleteBoard(int totalRequiredHits, float remainingTime);
    void RefreshBoard();
    void ReportMonsterDefeated();

    // 씬 관리
    void LoadScene(string sceneName);
    void RestartGame();
}
