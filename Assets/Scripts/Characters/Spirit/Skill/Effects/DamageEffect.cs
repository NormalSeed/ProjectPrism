using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class DamageEffect : ISkillEffect
{
    public string EffectName => "데미지 입히기";
    public int damageAmount;
    public bool scaleWithAtk;

    public async UniTask ExecuteAsync(SkillContext context)
    {
        int finalDamage = damageAmount;
        if (scaleWithAtk && context.Caster != null)
        {
            finalDamage += context.Caster.atk;
        }

        context.MonsterService?.TakeDamage(finalDamage);
        Debug.Log($"[Skill] {finalDamage} 데미지 효과 적용");
        await UniTask.Yield();
    }
}
