using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Piece, Effect, Passive }

[CreateAssetMenu(fileName = "NewItem", menuName = "ProjectPrism/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    public ItemRarity rarity;
    public ItemType itemType;
}
