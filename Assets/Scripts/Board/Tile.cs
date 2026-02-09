using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PieceType { None, Mirror, Prism, Obstacle, Emitter, Crystal }

public class Tile : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private GameObject obstacleIcon;
    [SerializeField] private GameObject crystalIcon;
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private GameObject emitterIcon;
    [SerializeField] private GameObject mirrorIcon;
    [SerializeField] private GameObject prismIcon;

    [Header("Status Visuals")]
    [SerializeField] private Image crystalBg; // 크리스탈 만족 시 색상 변경용
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color satisfiedColor = Color.green;

    private int x, y;
    private PieceType currentType = PieceType.None;
    private Action<int, int> onClickCallback;

    public void Init(int _x, int _y, Action<int, int> onClick)
    {
        x = _x;
        y = _y;
        onClickCallback = onClick;
    }

    public void SetState(PieceType type, CrystalData crystal = null, Vector2Int? dir = null)
    {
        currentType = type;

        // 아이콘 활성화/비활성화 제어
        obstacleIcon.SetActive(type == PieceType.Obstacle);
        crystalIcon.SetActive(type == PieceType.Crystal);
        emitterIcon.SetActive(type == PieceType.Emitter);
        mirrorIcon.SetActive(type == PieceType.Mirror);
        prismIcon.SetActive(type == PieceType.Prism);

        if (type == PieceType.Crystal && crystal != null)
        {
            // 목표치에서 현재 횟수를 뺀 '남은 횟수'를 표시
            crystalText.text = crystal.RemainingHits.ToString();

            // 남은 횟수가 0이면 강조 색상 적용
            if (crystalBg != null)
            {
                crystalBg.color = crystal.IsSatisfied ? satisfiedColor : normalColor;
            }
        }
        else if (crystalText != null)
        {
            crystalText.text = "";
        }

        // 광원(Emitter) 방향 설정
        if (type == PieceType.Emitter && dir.HasValue)
        {
            float angle = Mathf.Atan2(-dir.Value.y, dir.Value.x) * Mathf.Rad2Deg;
            emitterIcon.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 모든 시각 요소 초기화
    public void ResetTile()
    {
        currentType = PieceType.None;
        SetState(PieceType.None);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 장애물이나 광원은 유저가 조작할 수 없음
        if (currentType == PieceType.Obstacle || currentType == PieceType.Emitter) return;
        onClickCallback?.Invoke(x, y);
    }
}
