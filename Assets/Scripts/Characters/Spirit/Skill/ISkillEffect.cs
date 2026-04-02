using Cysharp.Threading.Tasks;

/// <summary>
/// 모든 스킬 효과의 기본이 되는 인터페이스
/// [SerializeReference]를 통해 다형성 구현이 가능
/// </summary>
public interface ISkillEffect
{
    string EffectName { get; }
    UniTask ExecuteAsync(SkillContext context);
}
