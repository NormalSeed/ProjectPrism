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
    [SubclassSelector][SerializeReference] private ISkillEffect _passiveEffect;

    public ISkillEffect PassiveEffect => _passiveEffect;

    [Header("Puzzle Pieces")]
    public List<PieceInventory> startingPieces = new List<PieceInventory>();

    private void OnValidate()
    {
        for (int i = 0; i < startingPieces.Count; i++)
        {
            var piece = startingPieces[i];

            if (piece.pieceType != PieceType.Mirror && piece.pieceType != PieceType.Prism)
                Debug.LogWarning($"{spiritName}: 정령은 Mirror 또는 Prism 타입의 기물만 가질 수 있습니다.");

            if (piece.count < 0)
            {
                piece.count = 0;
                startingPieces[i] = piece;
            }
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
