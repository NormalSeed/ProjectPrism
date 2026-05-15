using NUnit.Framework;
using UnityEngine;

public class SpiritDataTests
{
    // RED: compile error until PassiveEffect property is added to SpiritData
    [Test]
    public void PassiveEffect_IsNullByDefault()
    {
        var spirit = ScriptableObject.CreateInstance<SpiritData>();
        Assert.IsNull(spirit.PassiveEffect);
        Object.DestroyImmediate(spirit);
    }

    [Test]
    public void GetPieceCount_ReturnsMirrorCount()
    {
        var spirit = ScriptableObject.CreateInstance<SpiritData>();
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Mirror, count = 5 });
        Assert.AreEqual(5, spirit.GetPieceCount(PieceType.Mirror));
        Object.DestroyImmediate(spirit);
    }

    [Test]
    public void GetPieceCount_ReturnsPrismCount()
    {
        var spirit = ScriptableObject.CreateInstance<SpiritData>();
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Prism, count = 2 });
        Assert.AreEqual(2, spirit.GetPieceCount(PieceType.Prism));
        Object.DestroyImmediate(spirit);
    }

    [Test]
    public void GetTotalPieceCount_SumsAllPieces()
    {
        var spirit = ScriptableObject.CreateInstance<SpiritData>();
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Mirror, count = 5 });
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Prism, count = 2 });
        Assert.AreEqual(7, spirit.GetTotalPieceCount());
        Object.DestroyImmediate(spirit);
    }

    [Test]
    public void SpiritData_CanHaveMorePiecesThanRarityLimit()
    {
        // VS-style: a 3-star spirit can hold 7 pieces (5 Mirror + 2 Prism)
        var spirit = ScriptableObject.CreateInstance<SpiritData>();
        spirit.rarity = SpiritRarity.ThreeStars;
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Mirror, count = 5 });
        spirit.startingPieces.Add(new PieceInventory { pieceType = PieceType.Prism, count = 2 });
        Assert.AreEqual(7, spirit.GetTotalPieceCount());
        Object.DestroyImmediate(spirit);
    }
}
