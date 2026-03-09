using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PieceButtonUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PieceType pieceType; // 인스펙터에서 Mirror/Prism 설정

    [Header("UI Elements")]
    [SerializeField] private Image pieceIcon;
    [SerializeField] private TextMeshProUGUI countText;
    // TODO: 선택 시 나타나는 외곽선
    //[SerializeField] private GameObject selectionFrame; 

    [SerializeField] private Button pieceButton;
    private IBoardService boardService;
    
    [Inject]
    public void Construct(IBoardService _boardService)
    {
        boardService = _boardService;
    }

    private void Start()
    {
        if (boardService != null)
        {
            // 보드의 기물 개수/선택 상태가 변할 때마다 호출됨
            boardService.OnPieceCountChanged += RefreshStatus;
            RefreshStatus();
        }

        pieceButton.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (boardService != null)
            boardService.OnPieceCountChanged -= RefreshStatus;
    }

    /// <summary>
    /// 수동 배치된 버튼의 초기 설정을 돕는 메서드
    /// </summary>
    public void Setup(PieceType type, Sprite icon)
    {
        pieceType = type;
        if (pieceIcon != null) pieceIcon.sprite = icon;
        RefreshStatus();
    }

    public void RefreshStatus()
    {
        if (boardService == null) return;

        int count = boardService.GetRemainingPieceCount(pieceType);
        bool isSelected = boardService.GetSelectedPieceType() == pieceType;

        // 1. 개수 텍스트 갱신
        if (countText != null) countText.text = $"x{count}";

        // 2. 버튼 활성화 및 투명도 제어 (개수가 0이면 상호작용 불가)
        var button = GetComponent<Button>();
        if (button != null) button.interactable = count > 0;

        if (pieceIcon != null)
            pieceIcon.color = count > 0 ? Color.white : new Color(1, 1, 1, 0.4f);

        // TODO: 강조 표시 추가 필요
        //// 3. 선택 강조 표시
        //if (selectionFrame != null)
        //    selectionFrame.SetActive(isSelected);
    }

    public void OnClick()
    {
        if (boardService != null)
        {
            boardService.OnPieceButtonClicked(pieceType);
        }
    }
}
