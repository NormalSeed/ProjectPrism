using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterView : MonoBehaviour
{
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public Button RegisterButton;
    public Button CancleButton;

    public void SetInteractable(bool state) => RegisterButton.interactable = state;
}
