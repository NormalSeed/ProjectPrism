using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class AddTimeEffect : ISkillEffect
{
    public string EffectName => "시간 연장";
    public float seconds;

    public async UniTask ExecuteAsync(SkillContext context)
    {
        if (context.GameService != null)
        {
            context.GameService.RemainingTime.Value += seconds;
            Debug.Log($"[Skill] {seconds}초 연장 효과 적용");
        }
        await UniTask.Yield();
    }
}
