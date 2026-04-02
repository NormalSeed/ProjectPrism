using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpiritInventoryTester : MonoBehaviour
{
    [Header("테스트용 정령 목록")]
    [SerializeField] private List<SpiritData> mockSpirits = new List<SpiritData>();

    private IInventoryService _inventoryService;

    [Inject]
    public void Construct(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    private IEnumerator Start()
    {
        if (_inventoryService == null)
        {
            Debug.LogError("[Tester] 인벤토리 서비스를 찾을 수 없습니다.");
            yield break;
        }

        yield return null;

        // 테스트 데이터 주입
        PopulateMockData();
    }

    private void PopulateMockData()
    {
        if (mockSpirits == null || mockSpirits.Count == 0)
        {
            Debug.LogWarning("[Tester] 주입할 테스트 정령 데이터가 없습니다. Inspector를 확인하세요.");
            return;
        }

        // 1. 인터페이스에 추가된 LoadOwnedSpirits 메서드를 사용하여 안전하게 데이터 주입
        // 이 메서드 내부에서 OnInventoryUpdated?.Invoke()가 실행되므로 UI가 자동으로 갱신
        _inventoryService.LoadOwnedSpirits(mockSpirits);

        Debug.Log($"[Tester] {mockSpirits.Count}마리의 가상 정령 데이터를 LoadOwnedSpirits를 통해 주입했습니다.");

        // 2. 필요하다면 팀 구성을 보드와 동기화 (보통 로드 직후 한 번 수행)
        _inventoryService.SyncTeamWithBoard();
    }
}
