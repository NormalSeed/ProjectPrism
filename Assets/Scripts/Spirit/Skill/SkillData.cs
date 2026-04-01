using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Puzzle/Skill/Hybrid Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Costs")]
    public int manaCost;
    public float cooldown;

    [Header("Effects List")]
    /// <summary>
    /// [SerializeReference]는 이 리스트에 ISkillEffect를 상속받은 
    /// 다양한 클래스 인스턴스를 직접 담을 수 있게 해줌
    /// 별도의 SO 파일 없이도 인스펙터에서 효과를 추가할 수 있음
    /// </summary>
    [SubclassSelector]
    [SerializeReference]
    public List<ISkillEffect> effects = new List<ISkillEffect>();

    public async UniTask UseAsync(SkillContext context)
    {
        Debug.Log($"<color=cyan>[Skill] {skillName} 발동!</color>");

        foreach (var effect in effects)
        {
            if (effect != null)
            {
                await effect.ExecuteAsync(context);
            }
        }
    }
}