using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public interface IAuthService
{
    UniTask<FirebaseUser> SignInAsync(string email, string password);
    UniTask<FirebaseUser> SignUpAsync(string email, string password);

    void SignOut();
    FirebaseUser currentUser { get; }
}
