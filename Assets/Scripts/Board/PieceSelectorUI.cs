using System.Collections.Generic;
using UnityEngine;

public class PieceSelectorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Transform container;
    [SerializeField] private PieceButtonUI pieceButtonPrefab;

    [Header("Piece Icons")]
    [SerializeField] private Sprite mirrorIcon;
    [SerializeField] private Sprite prismIcon;

    private List<PieceButtonUI> spawnedButtons = new List<PieceButtonUI>();

    /// <summary>
    /// 현재 남은 기물 상태에 따라 버튼 리스트를 갱신하는 메서드
    /// </summary>
    public void Refresh(Dictionary<PieceType, int> remainingPieces, PieceType selectedType)
    {
        // 간단한 구현을 위해 전체 삭제 후 다시 생성 (풀링 권장)
        foreach (var btn in spawnedButtons) Destroy(btn.gameObject);
        spawnedButtons.Clear();

        foreach (var pair in remainingPieces)
        {
            if (pair.Value <= 0 && pair.Key != selectedType) continue;
            if (pair.Key != PieceType.Mirror && pair.Key != PieceType.Prism) continue;

            var newBtn = Instantiate(pieceButtonPrefab, container);
            Sprite icon = (pair.Key == PieceType.Mirror) ? mirrorIcon : prismIcon;

            // 버튼 초기화
            newBtn.Setup(pair.Key, pair.Value, icon, (type) => {
                boardManager.OnPieceButtonClicked(type);
            });

            // 선택 상태 표시
            newBtn.SetSelection(pair.Key == selectedType);
            spawnedButtons.Add(newBtn);
        }
    }
}

