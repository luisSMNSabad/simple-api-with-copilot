// Services/RoleService.cs
using Microsoft.AspNetCore.Identity;
using SecureApp.Constants;
using SecureApp.Models;

namespace SecureApp.Services;

public interface IRoleService
{
    Task<bool> AssignUserRole(string userId, string role);
    Task<bool> RemoveUserRole(string userId, string role);
    Task<IList<string>> GetUserRoles(string userId);
    Task EnsureRolesExist();
}

public class RoleService : IRoleService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> AssignUserRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        return true;
    }

    public async Task<bool> RemoveUserRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        return true;
    }

    public async Task<IList<string>> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task EnsureRolesExist()
    {
        foreach (var role in Roles.AllRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}