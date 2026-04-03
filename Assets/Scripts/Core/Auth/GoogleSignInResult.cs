using Firebase.Auth;

public class GoogleSignInResult
{
    public FirebaseUser User { get; set; }
    public bool IsNewUser { get; set; }
}
