using UnityEngine;

public class NewUserSetupPresenter : MonoBehaviour
{
    [SerializeField] private SpiritData _starterSpirit;

    private NewUserSetupView _view;

    private void Start()
    {
        _view = GetComponent<NewUserSetupView>();

        if (PlayerPrefs.GetInt(GameConsts.IsNewUserKey, 0) == 1)
        {
            _view.Show();
            _view.ConfirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnConfirmClicked()
    {
        string nickname = _view.NicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            _view.ShowError("닉네임을 입력해주세요.");
            return;
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            _view.ShowError("닉네임은 2~10자 사이여야 합니다.");
            return;
        }

        _view.SetInteractable(false);

        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.DeleteKey(GameConsts.IsNewUserKey);

        if (_starterSpirit != null)
            PlayerPrefs.SetString(GameConsts.SelectedSpiritKey, _starterSpirit.name);

        PlayerPrefs.Save();

        Debug.Log($"[NewUser] 닉네임 설정 완료: {nickname}. 튜토리얼을 시작합니다.");

        _view.Hide();
        StartTutorial();
    }

    private void StartTutorial()
    {
        // TODO: 튜토리얼 시퀀스 트리거
    }
}
