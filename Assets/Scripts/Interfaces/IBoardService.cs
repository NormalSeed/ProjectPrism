using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBoardService
{
    // 레이아웃 및 조회
    void UpdateBoardLayOut();
    Vector3 GetWorldPosition(int x, int y);
    Tile GetTile(int x, int y);

    // 게임 로직
    void SimulateCurrentLight();
    void SetTeam(List<SpiritData> team);

    // 기물 상호작용 및 인벤토리 조회
    void OnPieceButtonClicked(PieceType type);
    void OnPieceDropped(Vector2 screenPos, PieceType type);
    int GetRemainingPieceCount(PieceType type);
    PieceType GetSelectedPieceType();
    // 기물 버튼이 개수 변화를 감지하기 위한 이벤트
    event Action OnPieceCountChanged;
}
