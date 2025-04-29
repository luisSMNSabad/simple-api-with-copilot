using SecureApp.Models.Auth;
using SecureApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureApp.Services;
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(User user, string password);
    Task<string> GenerateJwtToken(User user);
}
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Find user by username
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            throw new ApplicationException("Invalid credentials");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new ApplicationException("Invalid credentials");
        }

        // Generate JWT token
        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.UserName,
            Roles = roles.ToList(),
            Expiration = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> RegisterAsync(User user, string password)
    {
        // Create user with hashed password
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new ApplicationException(
                $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return true;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

        // Add roles to claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
