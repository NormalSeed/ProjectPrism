using UnityEngine;
[CreateAssetMenu(fileName = "NewMonster", menuName = "Puzzle/MonsterData")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public Sprite monsterSprite;
    public int maxHp;
    public int baseReward;

    [Header("Visuals")]
    public Color themeColor = Color.white;
}
