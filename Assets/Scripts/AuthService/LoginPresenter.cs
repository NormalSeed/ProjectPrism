using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using VContainer;

public class LoginPresenter : MonoBehaviour
{
    LoginView view;

    IAuthService authService;
    UserDataModel userModel;

    [Inject]
    public void Construct(IAuthService _authService, UserDataModel _userModel)
    {
        authService = _authService;
        userModel = _userModel;
    }

    void Start()
    {
        view = GetComponent<LoginView>();

        view.LoginButton.OnClickAsAsyncEnumerable()
            .Subscribe(_ => OnLoginClicked().Forget());
        view.ToRegisterButton.onClick.AddListener(PopUpRegisterPannel);
    }

    async UniTaskVoid OnLoginClicked()
    {
        view.SetInteractable(false);
        try
        {
            var user = await authService.SignInAsync(view.EmailInput.text, view.PasswordInput.text);
            userModel.Email = user.Email; // Model 업데이트
            Debug.Log("로그인 성공!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그인 실패: {e.Message}");
        }
        finally
        {
            view.SetInteractable(true);
        }
    }

    void PopUpRegisterPannel()
    {
        if (!view.RegisterPannel.activeSelf)
        {
            view.RegisterPannel.SetActive(true);
        }
    }
}
