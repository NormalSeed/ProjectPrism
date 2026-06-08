using VContainer;
using VContainer.Unity;

public class MainMenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<MainMenuPresenter>();
        builder.RegisterComponentInHierarchy<SpiritSelectPanelPresenter>();
        builder.RegisterComponentInHierarchy<NewUserSetupPresenter>();
    }
}
