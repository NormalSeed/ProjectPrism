using VContainer;
using VContainer.Unity;

public class BoardLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 테스트용 클래스 등록
        builder.RegisterComponentInHierarchy<SpiritInventoryTester>();

        // 퍼즐 서비스 등록
        builder.Register<AstarSolver>(Lifetime.Singleton);
        builder.Register<PuzzlePathFinder>(Lifetime.Singleton);

        // 핵심 로직 매니저 등록
        builder.RegisterComponentInHierarchy<GameManager>().As<IGameService>();
        builder.RegisterComponentInHierarchy<BoardManager>().As<IBoardService>();
        builder.RegisterComponentInHierarchy<SpiritInventoryManager>().As<IInventoryService>();
        builder.RegisterComponentInHierarchy<MonsterManager>().As<IMonsterService>();

        // UI 컨트롤러 등록
        builder.RegisterComponentInHierarchy<SpiritInventoryUI>();
        builder.RegisterComponentInHierarchy <PieceSelectorUI>();
        builder.RegisterComponentInHierarchy<RightUI>();
        builder.RegisterComponentInHierarchy<MonsterBattleUI>();
    }
}
