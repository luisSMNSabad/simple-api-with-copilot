namespace SecureApp.Models.Auth;

public class LoginResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public DateTime Expiration { get; set; }
}
