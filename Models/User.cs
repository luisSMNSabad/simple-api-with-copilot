using Microsoft.AspNetCore.Identity;

namespace SecureApp.Models;
public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}