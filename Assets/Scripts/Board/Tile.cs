using System;
using System.Collections.Generic;
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

    [Header("Light Segements (UI Images)")]
    [SerializeField] private GameObject lightHorizontal;
    [SerializeField] private GameObject lightVertical;
    [SerializeField] private GameObject lightDiagonal1;
    [SerializeField] private GameObject lightDiagonal2;
    [SerializeField] private GameObject lightTopLeft;
    [SerializeField] private GameObject lightTopRight;
    [SerializeField] private GameObject lightBottomLeft;
    [SerializeField] private GameObject lightBottomRight;

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
        ClearLight();
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

    /// <summary>
    /// 들어오는 방향과 나가는 방향을 분석해 빛 이미지 활성화
    /// </summary>
    /// <param name="moveDir"></param>
    public void SetLight(Vector2Int inDir, Vector2Int outDir)
    {
        // 직선 경로 (들어온 방향과 나가는 방향이 같음)
        if (inDir == outDir)
        {
            if (inDir.x != 0 && inDir.y == 0) lightHorizontal?.SetActive(true);
            else if (inDir.x == 0 && inDir.y != 0) lightVertical?.SetActive(true);
            else if (inDir.x == inDir.y) lightDiagonal1?.SetActive(true); // [/]
            else if (inDir.x == -inDir.y) lightDiagonal2?.SetActive(true); // [\]
            return;
        }

        // 굴절/반사 경로 (90도 꺾임쇠 위주 처리)
        // 들어온 쪽(반대 방향)과 나가는 쪽을 연결
        HashSet<Vector2Int> connections = new HashSet<Vector2Int> { -inDir, outDir };

        if (connections.Contains(Vector2Int.up) && connections.Contains(Vector2Int.right)) lightTopLeft?.SetActive(true);
        else if (connections.Contains(Vector2Int.up) && connections.Contains(Vector2Int.left)) lightTopRight?.SetActive(true);
        else if (connections.Contains(Vector2Int.down) && connections.Contains(Vector2Int.right)) lightBottomLeft?.SetActive(true);
        else if (connections.Contains(Vector2Int.down) && connections.Contains(Vector2Int.left)) lightBottomRight?.SetActive(true);

        // 프리즘의 45도 굴절은 상황에 따라 Diagonal 이미지를 추가로 조합하거나 전용 꺾임쇠를 더 만들 수 있습니다.
        // 현재는 90도 꺾임 우선 처리.
    }

    public void ClearLight()
    {
        lightHorizontal?.SetActive(false);
        lightVertical?.SetActive(false);
        lightDiagonal1?.SetActive(false);
        lightDiagonal2?.SetActive(false);
        lightTopLeft?.SetActive(false);
        lightTopRight?.SetActive(false);
        lightBottomLeft?.SetActive(false);
        lightBottomRight?.SetActive(false);
    }

    // 모든 시각 요소 초기화
    public void ResetTile()
    {
        currentType = PieceType.None;
        SetState(PieceType.None);
        ClearLight();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 장애물이나 광원은 유저가 조작할 수 없음
        if (currentType == PieceType.Obstacle || currentType == PieceType.Emitter) return;
        onClickCallback?.Invoke(X, Y);
    }
}
