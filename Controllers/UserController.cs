using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureApp.Constants;
using SecureApp.Models;
using SecureApp.Repositories;
using SecureApp.Services;

namespace SecureApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly InputValidator _validator;

    public UserController(IUserRepository userRepository, InputValidator validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> ProcessForm([FromForm] string username, [FromForm] string email)
    {
        // Validate inputs
        var usernameValidation = _validator.ValidateUsername(username);
        if (!usernameValidation.IsValid)
        {
            return BadRequest(usernameValidation.ErrorMessage);
        }

        var emailValidation = _validator.ValidateEmail(email);
        if (!emailValidation.IsValid)
        {
            return BadRequest(emailValidation.ErrorMessage);
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetUserByUsernameAsync(usernameValidation.SanitizedValue);
        if (existingUser != null)
        {
            return BadRequest("Username already exists");
        }

        // Create new user
        var user = new User
        {
            UserName = usernameValidation.SanitizedValue,
            Email = emailValidation.SanitizedValue
        };

        var success = await _userRepository.CreateUserAsync(user);
        if (!success)
        {
            return StatusCode(500, "Failed to create user");
        }

        return Ok("User created successfully");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term cannot be empty");
        }

        // Sanitize search term
        var sanitizedTerm = _validator.RemoveDangerousCharacters(term);

        var users = await _userRepository.SearchUsersAsync(sanitizedTerm);
        return Ok(users);
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Available to any authenticated user
        return Ok("User profile data");
    }

    [HttpGet("sensitive-data")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.User}")] // Multiple roles
    public IActionResult GetSensitiveData()
    {
        return Ok("Sensitive data");
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult GetAdminData()
    {
        return Ok("Admin only data");
    }
}