using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpiritInventoryManager : MonoBehaviour, IInventoryService
{
    [Header("Data Collection")]
    [SerializeField] private List<SpiritData> _allOwnedSpirits = new List<SpiritData>();

    [Header("Team Setup")]
    [SerializeField] private List<SpiritData> _selectedTeam = new List<SpiritData>();
    private readonly int _maxTeamSize = 3;

    private IBoardService _boardService;

    public event Action OnInventoryUpdated;

    [Inject]
    public void Construct(IBoardService boardService)
    {
        _boardService = boardService;
    }

    private void Start()
    {
        SyncTeamWithBoard();
    }

    /// <summary>
    /// 정령을 팀에 추가하거나 제거하는 메서드
    /// </summary>
    public void ToggleSpiritSelection(SpiritData spirit)
    {
        if (_selectedTeam.Contains(spirit))
        {
            _selectedTeam.Remove(spirit);
        }
        else
        {
            if (_selectedTeam.Count < _maxTeamSize)
            {
                _selectedTeam.Add(spirit);
            }
            else
            {
                Debug.Log("정령을 더이상 편성할 수 없습니다.");
                return;
            }
        }

        SyncTeamWithBoard();
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// 현재 팀 구성을 보드에 동기화하는 메서드
    /// </summary>
    public void SyncTeamWithBoard()
    {
        if (_boardService != null)
        {
            _boardService.SetTeam(new List<SpiritData>(_selectedTeam));
            Debug.Log($"[SpiritInventory] 팀 동기화 완료: {_selectedTeam.Count}마리 출전 준비.");
        }
    }

    public List<SpiritData> GetOwnedSpirits() => _allOwnedSpirits;
    public bool IsSpiritSelected(SpiritData spirit) => _selectedTeam.Contains(spirit);

    public int CurrentTeamCount => _selectedTeam.Count;

    public void LoadOwnedSpirits(List<SpiritData> spirits)
    {
        if (spirits == null)
        {
            Debug.Log("allOwnedSpirits에 정령이 존재하지 않습니다.");
            return;
        }

        _allOwnedSpirits = new List<SpiritData>(spirits);
        OnInventoryUpdated?.Invoke();

        Debug.Log($"[Inventory] {_allOwnedSpirits.Count}개의 정령 데이터가 성공적으로 로드되었습니다.");
    }
}
