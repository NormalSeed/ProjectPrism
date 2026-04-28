using VContainer;
using VContainer.Unity;

public class BoardLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 씬 전환 서비스
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);

        // 퍼즐 서비스 등록
        builder.Register<AstarSolver>(Lifetime.Singleton);
        builder.Register<PuzzlePathFinder>(Lifetime.Singleton);

        // 핵심 로직 매니저 등록
        builder.RegisterComponentInHierarchy<GameManager>().As<IGameService>();
        builder.RegisterComponentInHierarchy<BoardManager>().As<IBoardService>();
        builder.RegisterComponentInHierarchy<ItemInventoryManager>().As<IItemInventoryService>();
        builder.RegisterComponentInHierarchy<MonsterManager>().As<IMonsterService>();

        // UI 컨트롤러 등록
        builder.RegisterComponentInHierarchy<NewUserSetupPresenter>();
        builder.RegisterComponentInHierarchy<PieceSelectorUI>();
        builder.RegisterComponentInHierarchy<RightUI>();
        builder.RegisterComponentInHierarchy<MonsterBattleUI>();
    }
}
