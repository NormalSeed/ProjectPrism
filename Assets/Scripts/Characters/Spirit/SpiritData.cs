using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpiritRarity
{
    ThreeStars = 3,
    FourStars = 4,
    FiveStars = 5,
}

public enum SpiritType
{
    Aqua,
    Flame,
    Nature,
    Light,
    Dark,
}

[Serializable]
public struct PieceInventory
{
    public PieceType pieceType;
    public int count;
}

[CreateAssetMenu(fileName = "NewSpirit", menuName = "Puzzle/SpiritData")]
public class SpiritData : ScriptableObject
{
    [Header("Basic Information")]
    public string spiritName;
    public Sprite spiritIcon;
    public SpiritRarity rarity = SpiritRarity.ThreeStars;
    public SpiritType type = SpiritType.Light;

    [Header("Stats")]
    public int hp;
    public int atk;
    public int def;

    [Header("Skills")]
    [TextArea(2, 5)]
    public string passiveSkillDescription;
    [TextArea(2, 5)]
    public string activeSkillDescription;

    [Header("Puzzle Pieces")]
    [Tooltip("레어도에 따라 사용 가능한 총 기물 수가 제한됨\n(3성: 1개, 4성: 2개, 5성: 3개)")]
    public List<PieceInventory> startingPieces = new List<PieceInventory>();

    // 에디터에서 데이터 변경 시 호출되는 검증 로직
    private void OnValidate()
    {
        // 레어도에 따른 최대 슬롯 계산: 3성=1, 4성=2, 5성=3
        int maxAllowedPieces = (int)rarity - 2;

        int currentTotalCount = 0;
        for (int i = 0; i < startingPieces.Count; i++)
        {
            var piece = startingPieces[i];

            // 1. 유효하지 않은 기물 타입 체크 (Mirror, Prism만 허용)
            if (piece.pieceType != PieceType.Mirror && piece.pieceType != PieceType.Prism)
            {
                Debug.LogWarning($"{spiritName}: 정령은 Mirror 또는 Prism 타입의 기물만 가질 수 있습니다.");
            }

            // 2. 개수가 음수가 되지 않도록 방지
            if (piece.count < 0)
            {
                piece.count = 0;
                startingPieces[i] = piece;
            }

            currentTotalCount += piece.count;
        }

        // 3. 총 기물 수 제한 체크
        if (currentTotalCount > maxAllowedPieces)
        {
            Debug.LogError($"{spiritName}은 {rarity} 등급이므로 기물 총합이 {maxAllowedPieces}개를 초과할 수 없습니다. (현재: {currentTotalCount}개)");
        }
    }

    /// <summary>
    /// 특정 타입의 기물이 몇 개 있는지 반환합니다.
    /// </summary>
    public int GetPieceCount(PieceType type)
    {
        foreach (var p in startingPieces)
        {
            if (p.pieceType == type) return p.count;
        }
        return 0;
    }

    /// <summary>
    /// 정령이 가진 모든 기물의 총합 개수를 반환합니다.
    /// </summary>
    public int GetTotalPieceCount()
    {
        int total = 0;
        foreach (var p in startingPieces)
        {
            total += p.count;
        }
        return total;
    }
}
