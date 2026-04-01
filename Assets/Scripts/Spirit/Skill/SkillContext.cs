using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

// --- 1. 스킬 실행 컨텍스트 ---
/// <summary>
/// 스킬 효과가 실행될 때 필요한 서비스와 데이터를 전달하는 메서드
/// </summary>
public class SkillContext
{
    public IGameService GameService { get; set; }
    public IMonsterService MonsterService { get; set; }
    public IBoardService BoardService { get; set; }
    public SpiritData Caster { get; set; }
}

// --- 2. 스킬 효과 인터페이스 ---
/// <summary>
/// 모든 스킬 효과의 기본이 되는 인터페이스
/// [SerializeReference]를 통해 다형성 구현이 가능
/// </summary>
public interface ISkillEffect
{
    // 효과의 이름을 인스펙터에 표시하기 위함 (선택사항)
    string EffectName { get; }
    UniTask ExecuteAsync(SkillContext context);
}

// --- 3. 구체적인 효과 클래스들 (일반 클래스) ---

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