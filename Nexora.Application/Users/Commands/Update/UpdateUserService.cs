using Microsoft.AspNetCore.Identity;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.Update;

public class UpdateUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UpdateResponse> UpdateUserHandler(UpdateUserCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
    }
    
}