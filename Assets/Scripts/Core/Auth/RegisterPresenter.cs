using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using VContainer;

public class RegisterPresenter : MonoBehaviour
{
    private RegisterView _view;
    private IEmailAuthService _authService;
    private UserDataModel _userDataModel;

    [Inject]
    public void Construct(IEmailAuthService authService, UserDataModel userDataModel)
    {
        _authService = authService;
        _userDataModel = userDataModel;
    }

    private void Start()
    {
        _view = GetComponent<RegisterView>();

        _view.RegisterButton.onClick.AddListener(() => OnRegisterClicked().Forget());
        _view.CancleButton.onClick.AddListener(CancelRegisterPanel);
    }

    private async UniTaskVoid OnRegisterClicked()
    {
        string email = _view.EmailInput.text;
        string password = _view.PasswordInput.text;

        _view.SetInteractable(false);

        try
        {
            var user = await _authService.SignUpAsync(email, password);
            _userDataModel.Email = user.Email;
            Debug.Log($"회원가입 성공: {user.Email}님 환영합니다!");
        }
        catch (Firebase.FirebaseException ex)
        {
            HandleRegisterError(ex);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"알 수 없는 오류: {ex.Message}");
        }
        finally
        {
            _view.SetInteractable(true);
        }
    }

    private void HandleRegisterError(Firebase.FirebaseException ex)
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

    private void CancelRegisterPanel()
    {
        gameObject.SetActive(false);
    }
}
