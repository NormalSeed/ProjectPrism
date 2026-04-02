using System;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameService
{
    [Header("Combat Balance")]
    [SerializeField] private int _damageMultiplier = 50;
    [SerializeField] private float _timeBonusScale = 10f;

    public ObservableProperty<int> StageLevel { get; } = new(1);
    public ObservableProperty<float> RemainingTime { get; } = new();
    public ObservableProperty<float> BoardDuration { get; } = new();

    public event Action<int> OnDamageCalculated;
    public event Action OnBoardClear;
    public event Action<int> OnBoardChanged;

    public bool isGameStarted { get; set; } = false;

    private void Start()
    {
        OnBoardClear += () =>
        {
            RefreshBoard();
        };

        BoardDuration.Value = 30f;
        RemainingTime.Value = BoardDuration.Value;
    }

    private void Update()
    {
        if (isGameStarted == false) return;

        if (RemainingTime.Value > 0)
        {
            RemainingTime.Value -= Time.deltaTime;
        }
        else
        {
            RefreshBoard();
        }
    }

    /// <summary>
    /// BoardManager에서 퍼즐 해결 시 필요한 데이터를 담아 호출하는 메서드
    /// </summary>
    /// <param name="totalRequiredHits">이번 스테이지에서 요구된 총 히트 수</param>
    /// <param name="remainingTime">남은 시간</param>
    public void CompleteBoard(int totalRequiredHits, float remainingTime)
    {
        int baseDamage = totalRequiredHits * _damageMultiplier;
        int timeBonus = Mathf.FloorToInt(remainingTime * _timeBonusScale);
        int finalDamage = baseDamage + timeBonus;

        Debug.Log($"<color=orange>[Game] 퍼즐 해결! 계산된 데미지: {finalDamage}</color>");

        OnDamageCalculated?.Invoke(finalDamage);
        OnBoardClear?.Invoke();
    }

    public void ReportMonsterDefeated()
    {
        StageLevel.Value++;
        Debug.Log($"<color=red> [Game] 몬스터 처치. 다음 스테이지 {StageLevel.Value} 시작.");
    }

    public void RefreshBoard()
    {
        OnBoardChanged?.Invoke(StageLevel.Value);
        RemainingTime.Value = BoardDuration.Value;
    }

    public void LoadScene(string name) => UnityEngine.SceneManagement.SceneManager.LoadScene(name);

    public void RestartGame()
    {
        StageLevel.Value = 1;
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
