using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Commands.Update;

public class UpdateUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UploadAvatarHandler _uploadAvatar;
    private readonly IApplicationDbContext _context;

    public UpdateUserService(UserManager<ApplicationUser> userManager, UploadAvatarHandler uploadAvatar, IApplicationDbContext context)
    {
        _context = context;
        _userManager = userManager;
        _uploadAvatar = uploadAvatar;
    }

    public async Task<UpdateResponse> UpdateUserHandler(UpdateUserCommand request)
    {
        var user = await _userManager.Users.Include(u=>u.Address).Include(u=>u.Avatar).FirstOrDefaultAsync(u=>u.Id == request.Id);
        Console.WriteLine("This is user: "+user);
        if (user == null)
        {
            throw new UserIsNotFoundException();
        }

        user.FirstName = request.Firstname ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.Email = request.Email ?? user.Email;
        if (request.Address != null && user.Address != null)
        {
            user.Address.Line1 = request.Address.Line1 ?? user.Address.Line1;
            user.Address.Line2 = request.Address.Line2 ?? user.Address.Line2;
            user.Address.City = request.Address.City ?? user.Address.City;
            user.Address.Country = request.Address.Country ?? user.Address.Country;
            user.Address.PostalCode = request.Address.PostalCode ?? user.Address.PostalCode;
        }

        if (request.Photo != null)
        {
            UploadAvatarCommand uploadAvatarCommand = new UploadAvatarCommand(user.Id, user, request.Photo);
            UploadAvatarResponse avatarResponse = await _uploadAvatar.UpdateAvatar(uploadAvatarCommand,user,user.Avatar);
            user.Avatar = avatarResponse.Avatar;
        }

        await _context.SaveChangesAsync();
         
        return new UpdateResponse(new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email,
            user.Avatar.Uri, user.Address.Line1));
    }
    
}