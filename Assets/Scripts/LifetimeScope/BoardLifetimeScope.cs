using VContainer;
using VContainer.Unity;

public class BoardLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<AstarSolver>(Lifetime.Singleton);
        builder.Register<PuzzlePathFinder>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<BoardManager>().As<IBoardService>();
    }
}
