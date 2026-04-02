using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SpiritInventoryUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform _slotContainerRect;
    [SerializeField] private SpiritSlotUI _slotPrefab;
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _inventoryPanel;

    private GridLayoutGroup _gridLayout;
    private const int Columns = 4;
    private float _slotSize;

    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI _teamCountText;
    [SerializeField] private Button _startButton;

    private CanvasGroup _group;

    private IInventoryService _inventoryService;

    private readonly List<SpiritSlotUI> _cachedSlotComponents = new List<SpiritSlotUI>();

    [Inject]
    public void Construct(IInventoryService inventoryService, IBoardService boardService)
    {
        _inventoryService = inventoryService;
    }

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();

        if (_slotContainerRect != null)
        {
            _gridLayout = _slotContainerRect.GetComponent<GridLayoutGroup>();
        }

        CanvasGroupExtensions.SetUIActivation(_group, false);
    }

    private void Start()
    {
        InitializeAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid InitializeAsync(CancellationToken ct)
    {
        await UniTask.NextFrame(ct);
        UpdateSlotLayout();

        if (_inventoryService != null)
        {
            _inventoryService.OnInventoryUpdated += RefreshUI;
            RefreshUI();
        }

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClicked);
        }

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnDestroy()
    {
        if (_inventoryService != null)
        {
            _inventoryService.OnInventoryUpdated -= RefreshUI;
        }
    }

    /// <summary>
    /// 컨테이너 너비를 바탕으로 GridLayout의 cellSize를 계산해서 적용시키는 메서드
    /// </summary>
    public void UpdateSlotLayout()
    {
        if (_gridLayout == null || _slotContainerRect == null) return;

        float containerWidth = _slotContainerRect.rect.width;
        float totalPadding = _gridLayout.padding.left + _gridLayout.padding.right;
        float totalSpacing = _gridLayout.spacing.x * (Columns - 1);
        float finalSize = (containerWidth - totalPadding - totalSpacing) / Columns;
        _slotSize = finalSize;

        _gridLayout.cellSize = new Vector2(finalSize, finalSize);

        Debug.Log($"[InventoryUI] 슬롯 레이아웃 업데이트 완료. 셀 크기: {finalSize}");
    }

    /// <summary>
    /// 현재 인벤토리 서비스의 상태를 바탕으로 UI를 다시 그려주는 메서드
    /// </summary>
    public void RefreshUI()
    {
        if (_inventoryService == null || _slotPrefab == null) return;

        var ownedSpirits = _inventoryService.GetOwnedSpirits();
        int spiritCount = ownedSpirits.Count;

        for (int i = 0; i < spiritCount; i++)
        {
            SpiritSlotUI slot;

            if (i < _cachedSlotComponents.Count)
            {
                slot = _cachedSlotComponents[i];
            }
            else if (i < _slotContainerRect.childCount)
            {
                slot = _slotContainerRect.GetChild(i).GetComponent<SpiritSlotUI>();
                _cachedSlotComponents.Add(slot);
            }
            else
            {
                slot = Instantiate(_slotPrefab, _slotContainerRect);
                _cachedSlotComponents.Add(slot);
            }

            slot.gameObject.SetActive(true);

            var spiritData = ownedSpirits[i];
            bool isSelected = _inventoryService.IsSpiritSelected(spiritData);

            slot.Bind(spiritData, isSelected, _slotSize, (data) =>
            {
                _inventoryService.ToggleSpiritSelection(data);
            });
        }

        for (int i = spiritCount; i < _slotContainerRect.childCount; i++)
        {
            _slotContainerRect.GetChild(i).gameObject.SetActive(false);
        }

        UpdateStatusUI();
    }

    private void UpdateStatusUI()
    {
        if (_teamCountText != null)
        {
            _teamCountText.text = $"Team Count : {_inventoryService.CurrentTeamCount} / 3";
        }

        if (_startButton != null)
        {
            _startButton.interactable = _inventoryService.CurrentTeamCount > 0;
        }
    }

    /// <summary>
    /// 테스트용 시작 버튼 클릭 시 호출. 추후 자동 시작으로 변경 필요
    /// </summary>
    private void OnStartButtonClicked()
    {
        Debug.Log($"[InventoryUI] {_inventoryService.CurrentTeamCount}마리의 정령과 함께 게임을 시작합니다.");
    }

    public void OnInventoryButtonClicke()
    {
        CanvasGroupExtensions.SetUIActivation(_group, true);
    }

    private void OnCloseButtonClicked()
    {
        CanvasGroupExtensions.SetUIActivation(_group, false);
    }
}
