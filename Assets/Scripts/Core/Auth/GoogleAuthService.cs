using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Google;
using UnityEngine;

public class GoogleAuthService : IGoogleAuthService
{
    private const string WebClientId = "504100776999-msmrac9um0p6kbt1m4nekhsdl5mppc01.apps.googleusercontent.com";

    private readonly FirebaseAuth _auth = FirebaseAuth.DefaultInstance;
    public FirebaseUser currentUser => _auth.CurrentUser;

    public GoogleAuthService()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = WebClientId,
            RequestIdToken = true,
            UseGameSignIn = false
        };
        Debug.Log("[Google Auth] GoogleSignIn 설정 완료");
    }

    public async UniTask<GoogleSignInResult> SignInWithGoogleAsync()
    {
        Debug.Log("[Google Auth] Step 1: Google 계정 선택 UI 시작");
        GoogleSignInUser googleUser;
        try
        {
            googleUser = await GoogleSignIn.DefaultInstance.SignIn().AsUniTask();
        }
        catch (GoogleSignIn.SignInException e)
        {
            throw new System.Exception($"[Google Auth] Google 로그인 실패: {e.Status}");
        }

        Debug.Log($"[Google Auth] Step 2: 계정 선택 완료, email = {googleUser.Email}");

        string idToken = googleUser.IdToken;
        if (string.IsNullOrEmpty(idToken))
            throw new System.Exception("[Google Auth] ID Token 획득 실패 — Firebase Console에서 Google 제공업체가 활성화됐는지 확인하세요.");

        Debug.Log("[Google Auth] Step 3: Firebase SignInWithCredential 시작");
        var credential = GoogleAuthProvider.GetCredential(idToken, null);
        var user = await _auth.SignInWithCredentialAsync(credential).AsUniTask();
        Debug.Log($"[Google Auth] Step 4: Firebase 로그인 완료, user = {user?.DisplayName}");

        bool isNewUser = user.Metadata != null &&
            user.Metadata.CreationTimestamp == user.Metadata.LastSignInTimestamp;

        Debug.Log($"[Google Auth] {(isNewUser ? "신규 등록" : "로그인")} 완료: {user.DisplayName}");
        return new GoogleSignInResult { User = user, IsNewUser = isNewUser };
    }

    public void SignOut()
    {
        _auth.SignOut();
        GoogleSignIn.DefaultInstance.SignOut();
        Debug.Log("[Google Auth] 로그아웃 완료");
    }
}
