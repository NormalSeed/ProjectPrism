using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public Button LoginButton;
    public Button ToRegisterButton;
    public GameObject RegisterPannel;

    public void SetInteractable(bool state) => LoginButton.interactable = state;
}
