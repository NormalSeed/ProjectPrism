/// <summary>
/// 스킬 효과가 실행될 때 필요한 서비스와 데이터를 전달하는 컨텍스트
/// </summary>
public class SkillContext
{
    public IGameService GameService { get; set; }
    public IMonsterService MonsterService { get; set; }
    public IBoardService BoardService { get; set; }
    public SpiritData Caster { get; set; }
}
