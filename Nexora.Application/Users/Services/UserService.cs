using Microsoft.AspNetCore.Http;
using Nexora.Application.Common;
using Nexora.Application.Interfaces.Repositories;
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
    
    private readonly AvatarService _avatarService;
    private readonly IUserRepository _userRepository;
    

    public UserService(IUserRepository userRepository, AvatarService avatarService)
    {
        _userRepository = userRepository;
        _avatarService = avatarService;
    }
    
    public async Task<IResult> PromoteUserHandler(string userId)
    {
        ApplicationUser? user = await _userRepository.GetUser(userId);
        if (user == null || user.Email ==null) throw new BadHttpRequestException("Cannot find user by id");
        var result = await _userRepository.AddRole(user, RoleNames.Admin);
        if (!result) throw new BadHttpRequestException("Failed to promote user");
        UserDto userDto = new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email, user.Avatar?.Uri,
            user.Address?.Line1);
        return Results.Ok(new {message = "user is fetched", data = userDto});
    }
    
    public async Task<IResult> UpdateUserHandler(UpdateUserCommand request)
    {
        ApplicationUser? user = await _userRepository.GetUser(request.Id);
        Console.WriteLine("This is user: "+user);
        if (user == null || user.Email==null)
        {
            throw new UserIsNotFoundException();
        }

        user.FirstName = request.Firstname ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.Email = request.Email ?? user.Email;
        if (request.Address != null && user.Address != null)
        {
            user.Address.Line1 = request.Address.Line1;
            user.Address.Line2 = request.Address.Line2 ?? user.Address.Line2;
            user.Address.City = request.Address.City;
            user.Address.Country = request.Address.Country;
            user.Address.PostalCode = request.Address.PostalCode ?? user.Address.PostalCode;
        }

        if (request.Photo != null)
        {
            UploadAvatarCommand uploadAvatarCommand = new UploadAvatarCommand(user.Id, user, request.Photo);
            UploadAvatarResponse avatarResponse = await _avatarService.UpdateAvatar(uploadAvatarCommand,user,user.Avatar!);
            user.Avatar = avatarResponse.Avatar;
        }


        return Results.Ok(new {message = "The user is updated", data = new UpdateResponse(new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email,
            user.Avatar?.Uri, user.Address?.Line1))});
    }
    public async Task<IResult> BanUserHandler(String id)
    {
        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user != null)
        {
            user.Banned = true;
            await _userRepository.UpdateUser(user);
        }
        
        if ((bool)user?.Banned) return Results.Ok(new {message = "The user is banned"});
        return Results.BadRequest(new {message = "Failed to ban user"});
    }
    public async Task<IResult> UnBanUserHandler(String id)
    {
        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), id);
        user.Banned = false;
        if (!user.Banned) return Results.Ok(new {message = "The user is unbanned"});
        await _userRepository.UpdateUser(user);
        return Results.BadRequest(new {message = "Failed to unban user"});
    }

    public async Task<IResult> DeleteUserHandler(string id)
    {
        bool result= await _userRepository.DeleteUser(id);
        if (!result) throw new BadHttpRequestException("Failed to find user by id");
        return Results.BadRequest(new {message = "Failed to find user"});
    }
    
    public async Task<IResult> GetUsersHandler(AllUserCommand request)
    {

        PagedResult<ApplicationUser> result= await _userRepository.GetAllUsers( request);
        if (!result.Items.Any() || result==null || result.Items==null) throw new BadHttpRequestException("Failed to fetch users");
        List<UserDto> userList = result.Items.OrderBy(u => u.CreatedAt).Skip((request.currentPage - 1) * 10)
            .Take(10)
            .Select(u =>
            {
                return new UserDto(u.Id, u.FirstName + " " + u.LastName, u.Email!, u.Avatar?.Uri, u.Address?.Line1);
            }).ToList();
            
        return Results.Ok(new {message = "Users are retrieved", data = new AllUserResponse(userList, request.currentPage, result.TotalCount)});

    }
}