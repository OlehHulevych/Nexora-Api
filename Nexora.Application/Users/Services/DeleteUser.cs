using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nexora.Application.Interfaces.Context;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Services;

public class DeleteUser
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUser(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> BanUserHandler(String id)
    {
        var user = await _userManager.FindByIdAsync(id);
        user.Banned = true;
        if (user.Banned)
        {
            return true;
        }

        return false;
    }
    public async Task<bool> UnBanUserHandler(String id)
    {
        var user = await _userManager.FindByIdAsync(id);
        user.Banned = false;
        if (!user.Banned)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteUserHandler(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            throw new BadHttpRequestException("Failed to find user by id");
        }
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}