using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

public class UserService : IUserService
{
    private readonly IAvatarService _avatarService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IAvatarService avatarService, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _avatarService = avatarService;
        _logger = logger;
    }

    public async Task<IResult> PromoteUserHandler(string userId)
    {
        _logger.LogInformation("Promoting user {UserId} to Admin", userId);

        ApplicationUser? user = await _userRepository.GetUser(userId);
        if (user == null || user.Email == null)
        {
            _logger.LogWarning("Promote failed — user {UserId} not found", userId);
            throw new BadHttpRequestException("Cannot find user by id");
        }

        var result = await _userRepository.AddRole(user, RoleNames.Admin);
        if (!result)
        {
            _logger.LogError("Failed to assign Admin role to user {UserId}", userId);
            throw new BadHttpRequestException("Failed to promote user");
        }

        _logger.LogInformation("User {UserId} promoted to Admin successfully", userId);
        UserDto userDto = new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email, user.Avatar?.Uri, user.Address?.Line1);
        return Results.Ok(new { message = "user is fetched", data = userDto });
    }

    public async Task<IResult> UpdateUserHandler(string id, UpdateUserCommand request)
    {
        _logger.LogInformation("Updating user {UserId}", id);

        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user == null || user.Email == null)
        {
            _logger.LogWarning("Update failed — user {UserId} not found", id);
            throw new UserIsNotFoundException();
        }

        user.FirstName = request.Firstname ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.Email = request.Email ?? user.Email;

        if (request.Address != null && user.Address != null)
        {
            _logger.LogInformation("Updating address for user {UserId}", id);
            user.Address.Line1 = request.Address.Line1;
            user.Address.Line2 = request.Address.Line2 ?? user.Address.Line2;
            user.Address.City = request.Address.City;
            user.Address.Country = request.Address.Country;
            user.Address.PostalCode = request.Address.PostalCode ?? user.Address.PostalCode;
        }

        if (request.Photo != null)
        {
            _logger.LogInformation("Updating avatar for user {UserId}", id);
            UploadAvatarCommand uploadAvatarCommand = new UploadAvatarCommand(user.Id, user, request.Photo);
            UploadAvatarResponse avatarResponse = await _avatarService.UpdateAvatar(uploadAvatarCommand, user, user.Avatar!);
            user.Avatar = avatarResponse.Avatar;
            _logger.LogInformation("Avatar updated for user {UserId}", id);
        }

        _logger.LogInformation("User {UserId} updated successfully", id);
        return Results.Ok(new
        {
            message = "The user is updated",
            data = new UpdateResponse(new UserDto(user.Id, user.FirstName + " " + user.LastName, user.Email,
                user.Avatar?.Uri, user.Address?.Line1))
        });
    }

    public async Task<IResult> BanUserHandler(string id)
    {
        _logger.LogInformation("Banning user {UserId}", id);

        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user == null)
        {
            _logger.LogWarning("Ban failed — user {UserId} not found", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }

        user.Banned = true;
        await _userRepository.UpdateUser(user);

        if (user.Banned)
        {
            _logger.LogInformation("User {UserId} banned successfully", id);
            return Results.Ok(new { message = "The user is banned" });
        }

        _logger.LogError("Failed to ban user {UserId}", id);
        return Results.BadRequest(new { message = "Failed to ban user" });
    }

    public async Task<IResult> UnBanUserHandler(string id)
    {
        _logger.LogInformation("Unbanning user {UserId}", id);

        ApplicationUser? user = await _userRepository.GetUser(id);
        if (user == null)
        {
            _logger.LogWarning("Unban failed — user {UserId} not found", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }

        user.Banned = false;
        await _userRepository.UpdateUser(user);

        if (!user.Banned)
        {
            _logger.LogInformation("User {UserId} unbanned successfully", id);
            return Results.Ok(new { message = "The user is unbanned" });
        }

        _logger.LogError("Failed to unban user {UserId}", id);
        return Results.BadRequest(new { message = "Failed to unban user" });
    }

    public async Task<IResult> DeleteUserHandler(string id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);

        bool result = await _userRepository.DeleteUser(id);
        if (!result)
        {
            _logger.LogWarning("Delete failed — user {UserId} not found", id);
            throw new BadHttpRequestException("Failed to find user by id");
        }

        _logger.LogInformation("User {UserId} deleted successfully", id);
        return Results.Ok(new { message = "User deleted successfully" });
    }

    public async Task<IResult> GetUsersHandler(AllUserCommand request)
    {
        _logger.LogInformation("Fetching users — page {Page}", request.currentPage);

        PagedResult<ApplicationUser> result = await _userRepository.GetAllUsers(request);
        if (!result.Items.Any())
        {
            _logger.LogWarning("No users found for page {Page}", request.currentPage);
            throw new BadHttpRequestException("Failed to fetch users");
        }

        List<UserDto> userList = result.Items
            .OrderBy(u => u.CreatedAt)
            .Skip((request.currentPage - 1) * 10)
            .Take(10)
            .Select(u => new UserDto(u.Id, u.FirstName + " " + u.LastName, u.Email!, u.Avatar?.Uri, u.Address?.Line1))
            .ToList();

        _logger.LogInformation("Fetched {Count} users out of {Total} — page {Page}", userList.Count, result.TotalCount, request.currentPage);
        return Results.Ok(new { message = "Users are retrieved", data = new AllUserResponse(userList, request.currentPage, result.TotalCount) });
    }
}