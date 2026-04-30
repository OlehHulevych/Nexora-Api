using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Exceptions;

namespace Nexora.Application.Users.Services;

public class UserService:IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly AvatarService _avatarService;

    public UserService(UserManager<ApplicationUser> userManager, IApplicationDbContext context, IUserBlobStorage uploadService, AvatarService avatarService)
    {
        _userManager = userManager;
        _context = context;
        _avatarService = avatarService;
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
            UploadAvatarResponse avatarResponse = await _avatarService.UpdateAvatar(uploadAvatarCommand,user,user.Avatar);
            user.Avatar = avatarResponse.Avatar;
        }

        await _context.SaveChangesAsync();
         
        return new UpdateResponse(new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email,
            user.Avatar.Uri, user.Address.Line1));
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
    
    public async Task<AllUserResponse> getUsersHandler(AllUserCommand request)
    {
        IQueryable<ApplicationUser> users;
        if (!request.role.IsNullOrEmpty())
        {
            users = (from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join avatar in _context.Avatars on user.Id equals avatar.UserId
                join address in _context.Addresses on user.Id equals address.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == request.role
                select user);
        }

        users = _userManager.Users.Include(u=>u.Address).Include(u=>u.Avatar).AsQueryable();
        var length = await users.CountAsync();
        List<UserDto> userList = await users.OrderBy(u => u.CreatedAt).Skip((request.currentPage - 1) * 10).Take(10)
            .Select(u=> new UserDto(u.Id, u.FirstName + " "+u.LastName, u.Email, u.Avatar.Uri, u.Address.Line1))
            .ToListAsync();
        return new AllUserResponse(userList, request.currentPage, length / 10);

    }
}