using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SpiritInventoryUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform slotContainerRect;   // 정령 슬롯이 배치될 부모 객체
    [SerializeField] private SpiritSlotUI slotPrefab;           // 개별 정령 슬롯 프리팹
    [SerializeField] private Button closeButton;                // 닫힘 버튼
    [SerializeField] private GameObject inventoryPanel;         // 닫힘 버튼으로 끄게 될 패널

    private GridLayoutGroup gridLayout;
    private const int columns = 4;
    private float slotSize;

    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI teamCountText;    // 팀 편성 인원 표시
    [SerializeField] private Button startButton;                // 게임 시작 버튼

    private CanvasGroup group;

    private IInventoryService inventoryService;

    // 생성된 슬롯 컴포넌트들을 담아두는 리스트
    private readonly List<SpiritSlotUI> cachedSlotComponents = new List<SpiritSlotUI>();

    [Inject]
    public void Construct(IInventoryService _inventoryService)
    {
        inventoryService = _inventoryService;
    }

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (slotContainerRect != null)
        {
            gridLayout = slotContainerRect.GetComponent<GridLayoutGroup>();
        }

        CanvasGroupExtensions.SetUIActivation(group, false);
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

        closeButton.onClick.AddListener(OnCloseButtonClicked);
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
        slotSize = finalSize;

        // cellSize를 설정
        gridLayout.cellSize = new Vector2(finalSize, finalSize);

        // UI 크기 조정


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

        // 1. 필요한 만큼 슬롯 생성 및 재사용
        for (int i = 0; i < spiritCount; i++)
        {
            SpiritSlotUI slot;

            if (i < cachedSlotComponents.Count)
            {
                // 캐시된 리스트에서 가져옴
                slot = cachedSlotComponents[i];
            }
            else if (i < slotContainerRect.childCount)
            {
                // 리스트에는 없지만 오브젝트가 있다면 가져와서 캐싱
                slot = slotContainerRect.GetChild(i).GetComponent<SpiritSlotUI>();
                cachedSlotComponents.Add(slot);
            }
            else
            {
                // 오브젝트도 없다면 새로 생성하고 캐싱
                slot = Instantiate(slotPrefab, slotContainerRect);
                cachedSlotComponents.Add(slot);
            }

            // 오브젝트 활성화 및 데이터 바인딩
            slot.gameObject.SetActive(true);

            var spiritData = ownedSpirits[i];
            bool isSelected = inventoryService.IsSpiritSelected(spiritData);

            slot.Bind(spiritData, isSelected, slotSize, (data) =>
            {
                inventoryService.ToggleSpiritSelection(data);
            });
        }

        // 2. 데이터 개수보다 많은 남은 오브젝트들은 비활성화 처리
        for (int i = spiritCount; i < slotContainerRect.childCount; i++)
        {
            slotContainerRect.GetChild(i).gameObject.SetActive(false);
        }

        // 3. 하단 상태 정보 업데이트
        UpdateStatusUI();
    }

    /// <summary>
    /// 팀 편성 상태 및 시작 버튼 활성화 여부를 업데이트하는 메서드. 시작 버튼 활성화쪽은 이후에 제거 필요함
    /// </summary>
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

    public void OnInventoryButtonClicke()
    {
        CanvasGroupExtensions.SetUIActivation(group, true);
    }

    private void OnCloseButtonClicked()
    {
        CanvasGroupExtensions.SetUIActivation(group, false);
    }
}
