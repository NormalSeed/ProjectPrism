using Cysharp.Threading.Tasks;

public interface IGoogleAuthService : IAuthService
{
    UniTask<GoogleSignInResult> SignInWithGoogleAsync();
}
