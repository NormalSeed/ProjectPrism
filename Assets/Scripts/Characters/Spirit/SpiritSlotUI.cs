using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 내 개별 정령 슬롯의 비주얼과 클릭 이벤트 관리하는 클래스
/// </summary>
public class SpiritSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;               // 정령 아이콘
    [SerializeField] private Image rarityBg;                // 등급별 배경색 이미지
    [SerializeField] private Image typeIcon;                // 속성 아이콘
    [SerializeField] private TextMeshProUGUI nameText;      // 정령 이름
    [SerializeField] private TextMeshProUGUI rarityText;    // 성급(★)
    [SerializeField] private GameObject selectionOverlay;   // 선택된 상태를 나타내는 하이라이트 효과

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI pieceInfoText; // TODO: 텍스트가 아니라 이미지 + 텍스트 형식으로 바꿔줘야 함


    [Header("UI Rects")]
    [SerializeField] private List<RectTransform> rects;

    // 원본 데이터를 저장하기 위한 구조체
    private struct OriginalData
    {
        public RectTransform rect;
        public Vector2 size;
        public float fontSize; // TMP인 경우 폰트 크기 저장
    }

    private List<OriginalData> _cachedData = new List<OriginalData>();
    private SpiritData boundData;
    private Action<SpiritData> onClickCallback;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitializeCache();

        // 마지막으로 패널 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 초기 디자인 시점의 크기와 폰트 사이즈 캐싱
    /// </summary>
    private void InitializeCache()
    {
        if (_isInitialized) return;

        if (rects != null)
        {
            foreach (var rt in rects)
            {
                if (rt == null) continue;

                var data = new OriginalData { rect = rt, size = rt.sizeDelta };

                // 해당 오브젝트에 텍스트가 있다면 폰트 크기도 캐싱
                var tmp = rt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    data.fontSize = tmp.fontSize;
                }

                _cachedData.Add(data);
            }
        }
        _isInitialized = true;
    }

    /// <summary>
    /// 데이터를 슬롯에 바인딩해주는 메서드
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isSelected"></param>
    /// <param name="onClick"></param>
    public void Bind(SpiritData data, bool isSelected, float slotSize, Action<SpiritData> onClick)
    {
        // Awake가 호출되지 않았을 경우를 대비해 초기화 확인
        InitializeCache();

        boundData = data;
        onClickCallback = onClick;

        nameText.text = data.spiritName;
        iconImage.sprite = data.spiritIcon;
        rarityText.text = new string('★', (int)data.rarity);
        atkText.text = $"ATK: {data.atk}";
        defText.text = $"DEF: {data.def}";

        // 기물 정보 요약 표시
        // TODO: 아이콘 추가 필요함
        if (data.startingPieces != null && data.startingPieces.Count > 0)
        {
            // Mirror 2, Prism 1 형식으로 한 줄 요약
            string info = string.Join(", ", data.startingPieces
                .Where(p => p.count > 0)
                .Select(p => $"{p.pieceType} x{p.count}"));
            pieceInfoText.text = string.IsNullOrEmpty(info) ? "기물 없음" : info;
        }
        else
        {
            pieceInfoText.text = "기물 없음";
        }

        // 선택 상태 업데이트
        if (selectionOverlay != null)
        {
            selectionOverlay.SetActive(isSelected);
        }

        UpdateVisualTheme(data);
        // 실시간으로 계산된 슬롯 사이즈에 맞춰 내부 요소 크기 재조정
        AdjustElementScale(slotSize);
    }

    public void AdjustElementScale(float size)
    {
        float referenceSize = 200f;
        float ratio = size / referenceSize;

        // 캐싱된 원본 데이터를 바탕으로 비율을 곱함 (중첩 계산 방지)
        foreach (var data in _cachedData)
        {
            if (data.rect == null) continue;

            // 크기(Width, Height) 조정
            data.rect.sizeDelta = data.size * ratio;

            // 텍스트인 경우 폰트 크기도 함께 조정
            var tmp = data.rect.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = data.fontSize * ratio;
            }
        }
    }

    private void UpdateVisualTheme(SpiritData data)
    {
        // 레어도 배경색 (테두리나 베이스 배경)
        if (rarityBg != null)
        {
            rarityBg.color = GetRarityColor(data.rarity);
        }

        // 속성별 아이콘 색상 혹은 슬롯 포인트 색상 설정
        if (typeIcon != null)
        {
            typeIcon.color = GetTypeColor(data.type);
        }
    }

    private Color GetRarityColor(SpiritRarity rarity)
    {
        return rarity switch
        {
            SpiritRarity.FiveStars => new Color(1f, 0.84f, 0f, 0.9f),   // 금색
            SpiritRarity.FourStars => new Color(0.75f, 0.4f, 1f, 0.9f), // 보라색
            SpiritRarity.ThreeStars => new Color(0.3f, 0.6f, 1f, 0.9f), // 파란색
            _ => Color.white
        };
    }

    private Color GetTypeColor(SpiritType type)
    {
        return type switch
        {
            SpiritType.Flame => new Color(1f, 0.3f, 0.3f),  // 빨강
            SpiritType.Aqua => new Color(0.3f, 0.7f, 1f),   // 파랑
            SpiritType.Nature => new Color(0.4f, 0.9f, 0.4f), // 초록
            SpiritType.Light => new Color(1f, 1f, 0.7f),    // 노랑/백색
            SpiritType.Dark => new Color(0.5f, 0.2f, 0.7f),  // 보라/검정
            _ => Color.white
        };
    }

    /// <summary>
    /// 버튼 컴포넌트의 OnClick 이벤트에서 호출할 메서드
    /// </summary>
    public void OnClick()
    {
        onClickCallback?.Invoke(boundData);
    }
}
