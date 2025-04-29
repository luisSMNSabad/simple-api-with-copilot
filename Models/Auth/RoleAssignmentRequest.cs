namespace SecureApp.Models.Auth;

public class RoleAssignmentRequest
{
    public string UserId { get; set; }
    public string Role { get; set; }
}