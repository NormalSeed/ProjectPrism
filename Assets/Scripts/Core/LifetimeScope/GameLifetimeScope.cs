using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<FirebaseInitializer>();
        builder.Register<IEmailAuthService, EmailAuthService>(Lifetime.Singleton);
        builder.Register<IGoogleAuthService, GoogleAuthService>(Lifetime.Singleton);
        builder.Register<UserDataModel>(Lifetime.Singleton);
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<RegisterPresenter>();
        builder.RegisterComponentInHierarchy<LoginPresenter>();
    }
}
