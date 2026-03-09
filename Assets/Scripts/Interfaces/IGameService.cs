using System;
using System.Collections;
using UnityEngine;

public interface IGameService
{
    // 스테이지가 변경되거나 클리어되었을 때 알릴 이벤트
    event Action<int> OnStageChanged;
    event Action OnGameClear;

    int CurrentStage { get; }

    // 스테이지 관리
    void CompleteStage();
    void MoveToNextStage();

    // 씬 관리
    void LoadScene(string sceneName);
    void RestartGame();
}
