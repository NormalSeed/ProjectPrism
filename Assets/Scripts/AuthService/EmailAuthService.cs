using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class EmailAuthService : IAuthService
{
    FirebaseAuth auth => FirebaseAuth.DefaultInstance;
    public FirebaseUser currentUser => auth.CurrentUser;

    public async UniTask<FirebaseUser> SignInAsync(string email, string password)
    {
        var result = await auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
        return result.User;
    }

    public async UniTask<FirebaseUser> SignUpAsync(string email, string password)
    {
        var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
        return result.User;
    }

    public void SignOut() => auth.SignOut();
}
