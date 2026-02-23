using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 내 개별 정령 슬롯의 비주얼과 클릭 이벤트 관리하는 클래스
/// </summary>
public class SpiritSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityBg;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private GameObject selectionOverlay;   // 선택된 상태를 나타내는 하이라이트 효과

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI pieceInfoText; // TODO: 텍스트가 아니라 이미지 + 텍스트 형식으로 바꿔줘야 함

    private SpiritData boundData;
    private Action<SpiritData> onClickCallback;

    /// <summary>
    /// 데이터를 슬롯에 바인딩해주는 메서드
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isSelected"></param>
    /// <param name="onClick"></param>
    public void Bind(SpiritData data, bool isSelected, Action<SpiritData> onClick)
    {
        boundData = data;
        onClickCallback = onClick;

        nameText.text = data.spiritName;
        iconImage.sprite = data.spiritIcon;
        rarityText.text = $"{(int)data.rarity}★";
        atkText.text = $"ATK: {data.atk}";

        // 기물 정보 요약 표시
        // TODO: 아이콘 추가 필요함
        string pieces = string.Join(", ", data.startingPieces.ConvertAll(p => $"{p.pieceType} x{p.count}"));
        pieceInfoText.text = pieces;

        // 선택 상태 업데이트
        selectionOverlay.SetActive(isSelected);

        // 레어도에 따른 배경색 변경
        if (rarityBg != null)
        {
            rarityBg.color = data.rarity switch
            {
                SpiritRarity.FiveStars => new Color(1f, 0.85f, 0.4f), // 골드
                SpiritRarity.FourStars => new Color(0.7f, 0.5f, 1f),   // 퍼플
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// 버튼 컴포넌트의 OnClick 이벤트에서 호출할 메서드
    /// </summary>
    public void OnClick()
    {
        onClickCallback?.Invoke(boundData);
    }
}
