using VContainer;
using VContainer.Unity;

public class BoardLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<BoardManager>().As<IBoardService>();
    }
}
