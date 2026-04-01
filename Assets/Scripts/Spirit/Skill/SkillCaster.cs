using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

public class SkillCaster : MonoBehaviour
{
    private IGameService _gameService;
    private IMonsterService _monsterService;
    private IBoardService _boardService;

    [Inject]
    public void Construct(IGameService gs, IMonsterService ms, IBoardService bs)
    {
        _gameService = gs;
        _monsterService = ms;
        _boardService = bs;
    }

    /// <summary>
    /// 실제 정령이 스킬을 사용할 때 호출하는 진입점
    /// </summary>
    public async UniTask CastSkill(SpiritData caster, SkillData skill)
    {
        if (skill == null) return;

        // 실행에 필요한 환경 정보 수집
        var context = new SkillContext
        {
            GameService = _gameService,
            MonsterService = _monsterService,
            BoardService = _boardService,
            Caster = caster
        };

        // 스킬 실행 (내부의 모든 이펙트가 순차 실행됨)
        await skill.UseAsync(context);
    }
}