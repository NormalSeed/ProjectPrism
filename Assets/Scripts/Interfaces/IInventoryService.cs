using System;
using System.Collections.Generic;

public interface IInventoryService
{
    event Action OnInventoryUpdated;                // UI 갱신을 위한 이벤트
    void ToggleSpiritSelection(SpiritData spirit);  // 
    void SyncTeamWithBoard();
    List<SpiritData> GetOwnedSpirits();
    bool IsSpiritSelected(SpiritData spirit);
    int CurrentTeamCount { get; }
}
