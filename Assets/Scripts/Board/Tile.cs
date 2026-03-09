using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PieceType { None, Mirror, Prism, Obstacle, Emitter, Crystal }

public class Tile : MonoBehaviour, IPointerClickHandler
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

    public int X { get; private set; }
    public int Y { get; private set; }

    private PieceType currentType = PieceType.None;
    private Action<int, int> onClickCallback;

    public void Init(int _x, int _y, Action<int, int> onClick)
    {
        X = _x;
        Y = _y;
        onClickCallback = onClick;
    }

    public void SetState(PieceType type, CrystalData crystal = null, Vector2Int? dir = null, int orientation = 0)
    {
        currentType = type;

        // 아이콘 활성화/비활성화 제어
        obstacleIcon.SetActive(type == PieceType.Obstacle);
        crystalIcon.SetActive(type == PieceType.Crystal);
        emitterIcon.SetActive(type == PieceType.Emitter);
        mirrorIcon.SetActive(type == PieceType.Mirror);
        prismIcon.SetActive(type == PieceType.Prism);

        // 거울/프리즘 방향 회전 적용
        if (type == PieceType.Mirror || type == PieceType.Prism)
        {
            GameObject targetIcon = (type == PieceType.Mirror) ? mirrorIcon : prismIcon;
            // orientation이 0이면 y 스케일 1 (정상), 1이면 y 스케일 -1 (위아래 반전)
            float yStack = (orientation == 0) ? 1f : -1f;
            targetIcon.transform.localScale = new Vector3(1f, yStack, 1f);
        }

        // 크리스탈 텍스트 및 배경 처리
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
        onClickCallback?.Invoke(X, Y);
    }
}
