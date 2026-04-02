using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ShuffleBoardEffect : ISkillEffect
{
    public string EffectName => "보드 셔플";

    public async UniTask ExecuteAsync(SkillContext context)
    {
        context.GameService?.RefreshBoard();
        Debug.Log("[Skill] 보드 셔플 효과 적용");
        await UniTask.Yield();
    }
}
