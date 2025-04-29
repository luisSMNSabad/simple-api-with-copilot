using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureApp.Services;
using SecureApp.Constants;

namespace SecureApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)] // Only admins can access this controller
public class AdminController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IAuthService _authService;

    public AdminController(IRoleService roleService, IAuthService authService)
    {
        _roleService = roleService;
        _authService = authService;
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        if (!Roles.AllRoles.Contains(role))
            return BadRequest("Invalid role");

        var success = await _roleService.AssignUserRole(userId, role);
        if (!success)
            return BadRequest("Failed to assign role");

        return Ok($"Role {role} assigned successfully");
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        var success = await _roleService.RemoveUserRole(userId, role);
        if (!success)
            return BadRequest("Failed to remove role");

        return Ok($"Role {role} removed successfully");
    }

    [HttpGet("user-roles/{userId}")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var roles = await _roleService.GetUserRoles(userId);
        return Ok(roles);
    }
}