using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PieceButtonUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PieceType pieceType;

    [Header("UI Elements")]
    [SerializeField] private Image pieceIcon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject selectionFrame; // 선택 시 나타나는 외곽선

    private Action<PieceType> onClickAction;
    private IBoardService boardService;

    [Inject]
    public void Construct(IBoardService _boardService)
    {
        boardService = _boardService;
    }

    /// <summary>
    /// 동적으로 생성될 때 기물 타입과 아이콘 등을 설정합니다. (정적 배치 시에도 호출 가능)
    /// </summary>
    public void Setup(PieceType type, int count, Sprite icon, Action<PieceType> onClick = null)
    {
        pieceType = type;
        if (pieceIcon != null) pieceIcon.sprite = icon;
        if (countText != null) countText.text = $"x{count}";

        onClickAction = onClick;

        // 개수가 0개면 버튼 비활성화 및 반투명 처리
        var button = GetComponent<Button>();
        if (button != null) button.interactable = count > 0;

        if (pieceIcon != null)
            pieceIcon.color = count > 0 ? Color.white : new Color(1, 1, 1, 0.4f);
    }

    /// <summary>
    /// 외부에서 개수만 실시간으로 업데이트할 때 사용합니다.
    /// </summary>
    public void UpdateCount(int count)
    {
        if (countText != null) countText.text = $"x{count}";

        var button = GetComponent<Button>();
        if (button != null) button.interactable = count > 0;

        if (pieceIcon != null)
            pieceIcon.color = count > 0 ? Color.white : new Color(1, 1, 1, 0.4f);
    }

    /// <summary>
    /// 외부에서 선택 상태를 시각적으로 변경할 때 사용합니다.
    /// </summary>
    public void SetSelection(bool isSelected)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(isSelected);
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Unity Button의 OnClick 이벤트에 연결하세요)
    /// </summary>
    public void OnClick()
    {
        // 1. 등록된 개별 콜백이 있다면 실행
        onClickAction?.Invoke(pieceType);

        // 2. 서비스 인터페이스를 통해 BoardManager에 알림
        if (boardService != null)
        {
            boardService.OnPieceButtonClicked(pieceType);
        }
    }
}
