using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SpiritInventoryUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform slotContainerRect;            // 정령 슬롯이 배치될 부모 객체
    [SerializeField] private SpiritSlotUI slotPrefab;           // 개별 정령 슬롯 프리팹

    private GridLayoutGroup gridLayout;
    private const int columns = 4;

    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI teamCountText;    // 팀 편성 인원 표시
    [SerializeField] private Button startButton;                // 게임 시작 버튼

    private IInventoryService inventoryService;

    [Inject]
    public void Construct(IInventoryService _inventoryService)
    {
        inventoryService = _inventoryService;
    }

    private void Awake()
    {
        if (slotContainerRect != null)
        {
            gridLayout = slotContainerRect.GetComponent<GridLayoutGroup>();
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        UpdateSlotLayout();

        if (inventoryService != null)
        {
            inventoryService.OnInventoryUpdated += RefreshUI;

            // 초기 화면 그리기
            RefreshUI();
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (inventoryService != null)
        {
            inventoryService.OnInventoryUpdated -= RefreshUI;
        }
    }

    /// <summary>
    /// 컨테이너 너비를 바탕으로 GridLayout의 cellsize를 계산해서 적용시키는 메서드
    /// </summary>
    public void UpdateSlotLayout()
    {
        if (gridLayout == null || slotContainerRect == null) return;

        // Content 영역의 실제 너비를 가져옴
        float containerWidth = slotContainerRect.rect.width;

        // 패딩과 가로 간격을 제외한 가용 공간을 계산
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (columns - 1);

        // 정사각형 슬롯을 위한 최종 크기 계산
        float finalSize = (containerWidth - totalPadding - totalSpacing) / columns;

        // cellSize를 설정
        gridLayout.cellSize = new Vector2(finalSize, finalSize);

        Debug.Log($"[InventoryUI] 슬롯 레이아웃 업데이트 완료. 셀 크기: {finalSize}");
    }

    /// <summary>
    /// 현재 인벤토리 서비스의 상태를 바탕으로 UI를 다시 그려주는 메서드
    /// </summary>
    public void RefreshUI()
    {
        if (inventoryService == null || slotPrefab == null) return;

        var ownedSpirits = inventoryService.GetOwnedSpirits();
        int spiritCount = ownedSpirits.Count;

        // 1. 기존 슬롯 중 부족한 만큼 생성하거나, 남는 것은 비활성화
        int currentChildCount = slotContainerRect.childCount;

        // 필요한 만큼 슬롯 활성화 및 바인딩
        for (int i = 0; i < spiritCount; i++)
        {
            SpiritSlotUI slot;

            if (i < currentChildCount)
            {
                // 기존 오브젝트 재사용
                slot = slotContainerRect.GetChild(i).GetComponent<SpiritSlotUI>();
                slot.gameObject.SetActive(true);
            }
            else
            {
                // 모자라면 새로 생성
                slot = Instantiate(slotPrefab, slotContainerRect);
            }

            // 데이터 바인딩
            var spiritData = ownedSpirits[i];
            bool isSelected = inventoryService.IsSpiritSelected(spiritData);

            slot.Bind(spiritData, isSelected, (data) =>
            {
                inventoryService.ToggleSpiritSelection(data);
            });
        }

        // 2. 남는 슬롯들은 비활성화 처리
        for (int i = spiritCount; i < slotContainerRect.childCount; i++)
        {
            slotContainerRect.GetChild(i).gameObject.SetActive(false);
        }

        // 3. 상태 UI 업데이트
        UpdateStatusUI();
    }

    private void UpdateStatusUI()
    {
        if (teamCountText != null)
        {
            teamCountText.text = $"편성된 정령 : {inventoryService.CurrentTeamCount} / 3";
        }

        if (startButton != null)
        {
            startButton.interactable = inventoryService.CurrentTeamCount > 0;
        }
    }

    /// <summary>
    /// 테스트용 시작 버튼 클릭시 게임을 시작해주는 메서드. 추후 게임 화면으로 들어갔을 때 버튼 없이 자동으로 시작하도록 변경해야 함
    /// </summary>
    private void OnStartButtonClicked()
    {
        Debug.Log($"[InventoryUI] {inventoryService.CurrentTeamCount}마리의 정령과 함께 게임을 시작합니다.");
        // TODO: 실제 게임 씬으로 넘어가거나 BoardManager의 스테이지 생성을 트리거하는 로직 구현 필요
    }
}
