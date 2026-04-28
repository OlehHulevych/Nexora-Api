using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Services;

public class Promote
{
    private readonly UserManager<ApplicationUser> _userManager;

    public Promote(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> promoteUserHandler(string userId)
    {
        ApplicationUser? user = await _userManager.Users.Include(u=>u.Address)
            .Include(u=>u.Avatar)
            .FirstOrDefaultAsync(u=>u.Id.Equals(userId));
        if (user == null)
        {
            throw new BadHttpRequestException("Cannot find user by id");
        }

        var result = await _userManager.AddToRoleAsync(user, RoleNames.Admin);
        if (!result.Succeeded)
        {
            throw new BadHttpRequestException("Failed to promote user");
        }

        UserDto userDto = new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email, user.Avatar.Uri,
            user.Address.Line1);
        return userDto;
    }
}