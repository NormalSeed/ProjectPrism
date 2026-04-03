using Firebase.Auth;

public interface IAuthService
{
    FirebaseUser currentUser { get; }
    void SignOut();
}
