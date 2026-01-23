public class UserDataModel
{
    public string Email { get; set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(Email);
}
