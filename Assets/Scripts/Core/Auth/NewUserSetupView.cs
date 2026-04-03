using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewUserSetupView : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _errorText;

    public TMP_InputField NicknameInput => _nicknameInput;
    public Button ConfirmButton => _confirmButton;

    public void Show()
    {
        _panel.SetActive(true);
        _errorText.gameObject.SetActive(false);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    public void ShowError(string message)
    {
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
    }

    public void SetInteractable(bool interactable)
    {
        _confirmButton.interactable = interactable;
        _nicknameInput.interactable = interactable;
    }
}
