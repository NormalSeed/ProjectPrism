using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class GameManager : MonoBehaviour, IGameService
{
    private int currentStage = 1;
    public int CurrentStage => currentStage;

    public event Action<int> OnStageChanged;
    public event Action OnGameClear;
    private float boardGenCooldown = 0.5f;

    private void Start()
    {
        OnGameClear += () =>
        {
            MoveToNextStage();
        };
    }

    /// <summary>
    /// 퍼즐 클리어시 호출되는 메서드
    /// </summary>
    public void CompleteStage()
    {
        Debug.Log($"<color=yellow>[Game] 스테이지 {currentStage} 클리어</color>");
        OnGameClear?.Invoke();
    }

    /// <summary>
    /// 다음 스테이지로 데이터를 갱신하고 보드를 재생성하는 메서드
    /// </summary>
    public void MoveToNextStage()
    {
        currentStage++;
        OnStageChanged?.Invoke(currentStage);
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[Game] 씬 이동 시도: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void RestartGame()
    {
        currentStage = 1;
        LoadScene(SceneManager.GetActiveScene().name);
    }
}
