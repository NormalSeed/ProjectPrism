using VContainer;
using VContainer.Unity;

public class BoardLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 씬 전환 서비스
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);

        // 런 관리 서비스
        builder.Register<IRunService, RunManager>(Lifetime.Singleton);

        // 던전 맵 서비스
        builder.Register<IDungeonMapService, DungeonMapManager>(Lifetime.Singleton);

        // 퍼즐 서비스 등록
        builder.Register<AstarSolver>(Lifetime.Singleton);
        builder.Register<PuzzlePathFinder>(Lifetime.Singleton);

        // 핵심 로직 매니저 등록
        builder.RegisterComponentInHierarchy<GameManager>().As<IGameService>();
        builder.RegisterComponentInHierarchy<BoardManager>().As<IBoardService>();
        builder.RegisterComponentInHierarchy<MonsterManager>().As<IMonsterService>();

        // 정령 인벤토리 등록
        builder.RegisterComponentInHierarchy<SpiritInventoryManager>().As<IInventoryService>();
        builder.RegisterComponentInHierarchy<SpiritInventoryUI>();

        // UI 컨트롤러 등록
        builder.RegisterComponentInHierarchy<PieceSelectorUI>();
        builder.RegisterComponentInHierarchy<RightUI>();
        builder.RegisterComponentInHierarchy<MonsterBattleUI>();
        builder.RegisterComponentInHierarchy<DungeonMapUI>();
    }
}
