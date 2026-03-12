using System;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameService
{
    [Header("Combat Balance")]
    [SerializeField] private int damageMultiplier = 50;
    [SerializeField] private float timeBonusScale = 10f;

    // 다른 매니저들이 구독할 상태들
    public ObservableProperty<int> StageLevel { get; } = new(1);

    // 데미지 계산에 필요한 정보
    public ObservableProperty<float> remainingTime { get; } = new();
    public ObservableProperty<float> boardDuration { get; } = new();

    // 보드 클리어 시 발생하는 이벤트 (데미지 계산 결과 전달)
    public event Action<int> OnDamageCalculated;
    public event Action OnBoardClear;
    public event Action<int> OnBoardChanged;

    public bool isGameStarted { get; set; } = false;

    private void Start()
    {
        // 보드 클리어 시 스테이지 전환 로직 연결
        OnBoardClear += () =>
        {
            RefreshBoard();
        };

        boardDuration.Value = 30f;
        remainingTime.Value = boardDuration.Value;
    }

    private void Update()
    {
        if (isGameStarted == false) return;

        if (remainingTime.Value > 0)
        {
            remainingTime.Value -= Time.deltaTime;
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
        // 1. 데미지 계산
        int baseDamage = totalRequiredHits * damageMultiplier;
        int timeBonus = Mathf.FloorToInt(remainingTime * timeBonusScale);
        int finalDamage = baseDamage + timeBonus;

        Debug.Log($"<color=orange>[Game] 퍼즐 해결! 계산된 데미지: {finalDamage}</color>");

        // 2. 데미지 이벤트 발행 (MonsterManager 등이 이를 수신)
        OnDamageCalculated?.Invoke(finalDamage);

        // 3. 게임 클리어 이벤트 발행
        OnBoardClear?.Invoke();
    }

    public void ReportMonsterDefeated()
    {
        StageLevel.Value++;
        Debug.Log($"<color=red> [Game] 몬스터 처치. 다음 스테이지 {StageLevel.Value} 시작.");

        RefreshBoard();
    }

    public void RefreshBoard()
    {
        // 스테이지 변경 이벤트 알림 (BoardManager가 이를 듣고 새 보드를 생성함)
        OnBoardChanged?.Invoke(StageLevel.Value);
        remainingTime.Value = boardDuration.Value;
    }

    public void LoadScene(string name) => UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    public void RestartGame()
    {
        StageLevel.Value = 1;
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
