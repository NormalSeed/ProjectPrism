using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class LoginPresenter : MonoBehaviour
{
    private LoginView _view;
    private IEmailAuthService _emailAuthService;
    private IGoogleAuthService _googleAuthService;
    private UserDataModel _userModel;
    private ISceneService _sceneService;
    private bool _isProcessing;

    [Inject]
    public void Construct(IEmailAuthService emailAuthService, IGoogleAuthService googleAuthService, UserDataModel userModel, ISceneService sceneService)
    {
        _emailAuthService = emailAuthService;
        _googleAuthService = googleAuthService;
        _userModel = userModel;
        _sceneService = sceneService;
    }

    private void Start()
    {
        _view = GetComponent<LoginView>();
        _view.LoginButton.onClick.AddListener(() => OnEmailLoginClicked().Forget());
        _view.GoogleLoginButton.onClick.AddListener(() => OnGoogleLoginClicked().Forget());
        _view.ToRegisterButton.onClick.AddListener(PopUpRegisterPanel);
    }

    private async UniTaskVoid OnEmailLoginClicked()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        _view.SetInteractable(false);
        try
        {
            var user = await _emailAuthService.SignInAsync(_view.EmailInput.text, _view.PasswordInput.text);
            _userModel.Email = user.Email;
            _userModel.DisplayName = user.DisplayName;
            Debug.Log("[Login] 이메일 로그인 성공!");
            OnLoginSuccess();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Login] 이메일 로그인 실패: {e.Message}");
        }
        finally
        {
            _isProcessing = false;
            _view.SetInteractable(true);
        }
    }

    private async UniTaskVoid OnGoogleLoginClicked()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        _view.SetInteractable(false);
        try
        {
            var result = await _googleAuthService.SignInWithGoogleAsync();
            _userModel.Email = result.User.Email;
            _userModel.DisplayName = result.User.DisplayName;
            _userModel.PhotoUrl = result.User.PhotoUrl?.ToString();
            _userModel.IsNewUser = result.IsNewUser;

            if (result.IsNewUser)
            {
                Debug.Log("[Login] 구글 계정 신규 등록 완료. 프로필 설정으로 이동합니다.");
                OnNewUserRegistered();
            }
            else
            {
                Debug.Log("[Login] 구글 로그인 성공.");
                OnLoginSuccess();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Login] 구글 로그인 실패: {e.Message}");
        }
        finally
        {
            _isProcessing = false;
            _view.SetInteractable(true);
        }
    }

    private void OnLoginSuccess()
    {
        _sceneService.LoadMainMenuScene();
    }

    private void OnNewUserRegistered()
    {
        PlayerPrefs.SetInt(GameConsts.IsNewUserKey, 1);
        PlayerPrefs.Save();
        _sceneService.LoadMainMenuScene();
    }

    private void PopUpRegisterPanel()
    {
        if (!_view.RegisterPannel.activeSelf)
            _view.RegisterPannel.SetActive(true);
    }
}
