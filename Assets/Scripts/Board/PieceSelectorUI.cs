using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PieceSelectorUI : MonoBehaviour
{
    [Header("Piece Buttons")]
    [SerializeField] private List<PieceButtonUI> pieceButtons = new List<PieceButtonUI>();

    private IBoardService boardService;

    [Inject]
    public void Construct(IBoardService _boardService)
    {
        boardService = _boardService;
    }

    private void Start()
    {
        // 만약 인스펙터에서 할당하지 않았다면 자식 오브젝트에서 자동으로 찾아옵니다.
        if (pieceButtons == null || pieceButtons.Count == 0)
        {
            pieceButtons = new List<PieceButtonUI>(GetComponentsInChildren<PieceButtonUI>());
        }

        if (boardService != null)
        {
            // 보드 데이터(정령 편성, 기물 개수 등)가 변경될 때 호출될 이벤트 구독
            boardService.OnPieceCountChanged += RefreshAllButtons;
        }
    }

    private void OnDestroy()
    {
        if (boardService != null)
        {
            boardService.OnPieceCountChanged -= RefreshAllButtons;
        }
    }

    /// <summary>
    /// 관리 중인 모든 버튼의 상태(개수, interactable, 선택 효과)를 갱신합니다.
    /// </summary>
    public void RefreshAllButtons()
    {
        foreach (var btn in pieceButtons)
        {
            if (btn != null)
            {
                // 각 버튼 내부에 구현된 RefreshStatus()를 호출하게 하거나,
                // 버튼이 스스로 이벤트를 구독하고 있다면 이 메서드는 생략 가능합니다.
                // 여기서는 확실한 갱신을 위해 명시적으로 호출할 수 있게 설계합니다.
                btn.RefreshStatus();
            }
        }
    }
}

