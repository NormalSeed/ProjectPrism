using Cysharp.Threading.Tasks;
using Firebase.Auth;

public class EmailAuthService : IEmailAuthService
{
    private FirebaseAuth _auth => FirebaseAuth.DefaultInstance;
    public FirebaseUser currentUser => _auth.CurrentUser;

    public async UniTask<FirebaseUser> SignInAsync(string email, string password)
    {
        var result = await _auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
        return result.User;
    }

    public async UniTask<FirebaseUser> SignUpAsync(string email, string password)
    {
        var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
        return result.User;
    }

    public void SignOut() => _auth.SignOut();
}
