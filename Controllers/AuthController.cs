using Microsoft.AspNetCore.Mvc;
using SecureApp.Services;
using SecureApp.Models.Auth;

namespace SecureApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly InputValidator _validator;

    public AuthController(IAuthService authService, InputValidator validator)
    {
        _authService = authService;
        _validator = validator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate input
            var usernameValidation = _validator.ValidateUsername(request.Username);
            if (!usernameValidation.IsValid)
            {
                return BadRequest(usernameValidation.ErrorMessage);
            }

            // Attempt login
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}