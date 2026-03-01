using System.Collections.Generic;
using System.Reflection;
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

    private void Start()
    {
        if (_inventoryService == null)
        {
            Debug.LogError("[Tester] 인벤토리 서비스를 찾을 수 없습니다.");
            return;
        }

        // 테스트 데이터 주입
        PopulateMockData();
    }

    private void PopulateMockData()
    {
        // SpiritInventoryManager의 private 리스트에 접근하기 위해 리플렉션을 사용하거나, Manager에 테스트용 메서드를 추가하여 데이터를 넣어줌
        if (_inventoryService is SpiritInventoryManager manager)
        {
            // 리플렉션을 사용하여 private 필드인 allOwnedSpirits에 직접 데이터를 주입
            var field = typeof(SpiritInventoryManager).GetField("allOwnedSpirits",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(manager, new List<SpiritData>(mockSpirits));
                Debug.Log($"[Tester] {mockSpirits.Count}마리의 가상 정령 데이터가 인벤토리에 주입되었습니다.");

                // 데이터가 바뀌었음을 UI에 알림
                // Manager 내부에서 이벤트를 발생시키는 public 메서드가 없으므로 
                // 강제로 Toggle을 한 번 시도하거나 이벤트를 수동으로 호출하는 로직이 필요할 수 있음
                manager.SyncTeamWithBoard();
            }
        }
    }
}
