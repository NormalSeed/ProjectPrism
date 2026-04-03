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
    [SerializeField] private GameObject _obstacleIcon;
    [SerializeField] private GameObject _crystalIcon;
    [SerializeField] private TextMeshProUGUI _crystalText;
    [SerializeField] private GameObject _emitterIcon;
    [SerializeField] private GameObject _mirrorIcon;
    [SerializeField] private GameObject _prismIcon;

    [Header("Light Segments (UI Images)")]
    [SerializeField] private GameObject _lightHorizontal;
    [SerializeField] private GameObject _lightVertical;
    [SerializeField] private GameObject _lightDiagonal1;
    [SerializeField] private GameObject _lightDiagonal2;
    [SerializeField] private GameObject _lightTopLeft;
    [SerializeField] private GameObject _lightTopRight;
    [SerializeField] private GameObject _lightBottomLeft;
    [SerializeField] private GameObject _lightBottomRight;

    [Header("Status Visuals")]
    [SerializeField] private Image _crystalBg;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _satisfiedColor = Color.green;

    public int X { get; private set; }
    public int Y { get; private set; }

    private PieceType _currentType = PieceType.None;
    private Action<int, int> _onClickCallback;

    public void Init(int x, int y, Action<int, int> onClick)
    {
        X = x;
        Y = y;
        _onClickCallback = onClick;
        ClearLight();
    }

    public void SetState(PieceType type, CrystalData crystal = null, Vector2Int? dir = null, int orientation = 0)
    {
        _currentType = type;

        _obstacleIcon.SetActive(type == PieceType.Obstacle);
        _crystalIcon.SetActive(type == PieceType.Crystal);
        _emitterIcon.SetActive(type == PieceType.Emitter);
        _mirrorIcon.SetActive(type == PieceType.Mirror);
        _prismIcon.SetActive(type == PieceType.Prism);

        if (type == PieceType.Mirror || type == PieceType.Prism)
        {
            GameObject targetIcon = (type == PieceType.Mirror) ? _mirrorIcon : _prismIcon;
            float yScale = (orientation == 0) ? 1f : -1f;
            targetIcon.transform.localScale = new Vector3(1f, yScale, 1f);
        }

        if (type == PieceType.Crystal && crystal != null)
        {
            _crystalText.text = crystal.RemainingHits.ToString();

            if (_crystalBg != null)
            {
                _crystalBg.color = crystal.IsSatisfied ? _satisfiedColor : _normalColor;
            }
        }
        else if (_crystalText != null)
        {
            _crystalText.text = "";
        }

        if (type == PieceType.Emitter && dir.HasValue)
        {
            float angle = Mathf.Atan2(-dir.Value.y, dir.Value.x) * Mathf.Rad2Deg;
            _emitterIcon.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// 들어오는 방향과 나가는 방향을 분석해 빛 이미지 활성화
    /// </summary>
    public void SetLight(Vector2Int inDir, Vector2Int outDir)
    {
        // 직진: 같은 방향
        if (inDir == outDir)
        {
            ActivateLightSegment(inDir);
            return;
        }

        bool inIsStraight = inDir.x == 0 || inDir.y == 0;
        bool outIsStraight = outDir.x == 0 || outDir.y == 0;

        // 거울: 수평/수직 사이의 90도 꺾임 → 코너 세그먼트
        if (inIsStraight && outIsStraight)
        {
            HashSet<Vector2Int> c = new HashSet<Vector2Int> { -inDir, outDir };
            if (c.Contains(Vector2Int.up) && c.Contains(Vector2Int.right)) _lightTopLeft?.SetActive(true);
            else if (c.Contains(Vector2Int.up) && c.Contains(Vector2Int.left)) _lightTopRight?.SetActive(true);
            else if (c.Contains(Vector2Int.down) && c.Contains(Vector2Int.right)) _lightBottomLeft?.SetActive(true);
            else if (c.Contains(Vector2Int.down) && c.Contains(Vector2Int.left)) _lightBottomRight?.SetActive(true);
            return;
        }

        // 프리즘: 45도 굴절 → 프리즘 아이콘으로 표현하므로 빛 세그먼트 표시 안 함
    }

    private void ActivateLightSegment(Vector2Int dir)
    {
        if (dir.x != 0 && dir.y == 0) _lightHorizontal?.SetActive(true);
        else if (dir.x == 0 && dir.y != 0) _lightVertical?.SetActive(true);
        else if (dir.x == dir.y) _lightDiagonal1?.SetActive(true);
        else if (dir.x == -dir.y) _lightDiagonal2?.SetActive(true);
    }

    public void ClearLight()
    {
        _lightHorizontal?.SetActive(false);
        _lightVertical?.SetActive(false);
        _lightDiagonal1?.SetActive(false);
        _lightDiagonal2?.SetActive(false);
        _lightTopLeft?.SetActive(false);
        _lightTopRight?.SetActive(false);
        _lightBottomLeft?.SetActive(false);
        _lightBottomRight?.SetActive(false);
    }

    public void ResetTile()
    {
        _currentType = PieceType.None;
        SetState(PieceType.None);
        ClearLight();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentType == PieceType.Obstacle || _currentType == PieceType.Emitter) return;
        _onClickCallback?.Invoke(X, Y);
    }
}
