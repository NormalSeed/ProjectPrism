using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpiritInventoryManager : MonoBehaviour, IInventoryService
{
    [Header("Data Collection")]
    [SerializeField] private List<SpiritData> allOwnedSpirits = new List<SpiritData>(); // 보유 중인 모든 정령

    [Header("Team Setup")]
    [SerializeField] private List<SpiritData> selectedTeam = new List<SpiritData>();    // 현재 선택된 팀
    private readonly int maxTeamSize = 3;   // 최대 3개체까지 구성 가능

    private IBoardService boardService;

    public event Action OnInventoryUpdated;

    [Inject]
    public void Contruct(IBoardService _boardService)
    {
        boardService = _boardService;
    }

    private void Start()
    {
        SyncTeamWithBoard();   
    }

    /// <summary>
    /// 정령을 팀에 추가하거나 제거하는 메서드
    /// </summary>
    /// <param name="spirit"></param>
    public void ToggleSpiritSelection(SpiritData spirit)
    {
        if (selectedTeam.Contains(spirit))
        {
            selectedTeam.Remove(spirit);
        }
        else
        {
            if (selectedTeam.Count < maxTeamSize)
            {
                selectedTeam.Add(spirit);
            }
            else
            {
                Debug.Log("정령을 더이상 편성할 수 없습니다.");
                return;
            }
        }

        SyncTeamWithBoard();
        // UI 상태 변경 알림
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// 현재 팀 구성을 보드에 동기화하는 메서드
    /// </summary>
    public void SyncTeamWithBoard()
    {
        if (boardService != null)
        {
            // 인벤토리 매니저의 내부 리스트 참조가 외부(보드)에서 직접 수정되어 발생하는 예기치 못한 사이드 이펙트를 방지함
            boardService.SetTeam(new List<SpiritData>(selectedTeam));
            Debug.Log($"[SpiritInventory] 팀 동기화 완료: {selectedTeam.Count}마리 출전 준비.");
        }
    }

    // 데이터 조회용 메서드
    public List<SpiritData> GetOwnedSpirits() => allOwnedSpirits;
    public bool IsSpiritSelected(SpiritData spirit) => selectedTeam.Contains(spirit);

    public int CurrentTeamCount => selectedTeam.Count;
}
