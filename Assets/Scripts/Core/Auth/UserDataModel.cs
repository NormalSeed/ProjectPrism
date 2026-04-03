public class UserDataModel
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string PhotoUrl { get; set; }
    public bool IsNewUser { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Email);
}
