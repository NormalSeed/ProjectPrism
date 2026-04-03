using Cysharp.Threading.Tasks;
using Firebase.Auth;

public interface IEmailAuthService : IAuthService
{
    UniTask<FirebaseUser> SignInAsync(string email, string password);
    UniTask<FirebaseUser> SignUpAsync(string email, string password);
}
