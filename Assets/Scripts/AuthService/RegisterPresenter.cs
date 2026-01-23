using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using VContainer;

public class RegisterPresenter : MonoBehaviour
{
    RegisterView view;

    IAuthService authService;
    UserDataModel userDataModel;

    [Inject]
    public void Construct(IAuthService _authService, UserDataModel _userDataModel)
    {
        authService = _authService;
        userDataModel = _userDataModel;
    }

    void Start()
    {
        view = GetComponent<RegisterView>();

        view.RegisterButton.onClick.AddListener(() => OnRegisterClicked().Forget());
        view.CancleButton.onClick.AddListener(CancleRegisterPannel);
    }

    async UniTaskVoid OnRegisterClicked()
    {
        string email = view.EmailInput.text;
        string password = view.PasswordInput.text;

        view.SetInteractable(false);

        try
        {
            // 2. Firebase 회원가입 요청
            var user = await authService.SignUpAsync(email, password);

            // 3. 성공 시 데이터 모델 업데이트
            userDataModel.Email = user.Email;
            Debug.Log($"회원가입 성공: {user.Email}님 환영합니다!");

            // 가입 성공 후 메인 로비로 이동하는 로직 등을 여기에 추가
        }
        catch (Firebase.FirebaseException ex)
        {
            // 4. Firebase 전용 에러 처리
            HandleRegisterError(ex);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"알 수 없는 오류: {ex.Message}");
        }
        finally
        {
            view.SetInteractable(true);
        }
    }

    void HandleRegisterError(Firebase.FirebaseException ex)
    {
        AuthError errorCode = (AuthError)ex.ErrorCode;
        string message = errorCode switch
        {
            AuthError.EmailAlreadyInUse => "이미 사용 중인 이메일입니다.",
            AuthError.InvalidEmail => "유효하지 않은 이메일 형식입니다.",
            AuthError.WeakPassword => "비밀번호가 너무 취약합니다.",
            _ => $"가입 실패: {ex.Message}"
        };
        Debug.LogError(message);
    }

    void CancleRegisterPannel()
    {
        gameObject.SetActive(false);
    }
}
