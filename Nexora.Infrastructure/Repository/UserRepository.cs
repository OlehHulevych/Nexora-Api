using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Common;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Domain.Constants;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserManager<ApplicationUser> userManager, ILogger<UserRepository> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> AddUser(ApplicationUser user, string? password, LoginProvider? loginProvider, GoogleJsonWebSignature.Payload? payload)
    {
        _logger.LogInformation("Creating user {Email} via {Provider}",
            user.Email, loginProvider?.ToString() ?? "Password");

        IdentityResult? result = null;

        if (password != null)
        {
            result = await _userManager.CreateAsync(user, password);
        }

        if (password == null && loginProvider == LoginProvider.Google && payload != null)
        {
            result = await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, new UserLoginInfo(
                LoginNames.Google,
                payload.Subject,
                LoginNames.Google));
        }

        if (result != null && result.Succeeded)
        {
            _logger.LogInformation("User {Email} created successfully with Id {UserId}", user.Email, user.Id);
            return true;
        }

        var errors = string.Join(", ", result?.Errors.Select(e => e.Description) ?? Array.Empty<string>());
        _logger.LogError("Failed to create user {Email}: {Errors}", user.Email, errors);
        return false;
    }

    public async Task<bool> UpdateUser(ApplicationUser user)
    {
        _logger.LogInformation("Updating user {UserId}", user.Id);
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} updated successfully", user.Id);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user {UserId}: {Errors}", user.Id, errors);
        }
        return result.Succeeded;
    }

    public async Task<ApplicationUser?> GetUser(string id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }
        return user;
    }

    public async Task<PagedResult<ApplicationUser>> GetAllUsers(AllUserCommand request)
    {
        _logger.LogInformation("Fetching all users — page {Page}, role filter: {Role}",
            request.currentPage, request.role ?? "none");

        IQueryable<ApplicationUser> query = _userManager.Users
            .Include(u => u.Address)
            .Include(u => u.Avatar)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.role))
        {
            _logger.LogInformation("Filtering users by role {Role}", request.role);
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.role);
            var ids = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => ids.Contains(u.Id));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((request.currentPage - 1) * 10)
            .Take(10)
            .ToListAsync();

        _logger.LogInformation("Fetched {Count} users out of {Total} — page {Page}",
            items.Count, total, request.currentPage);

        return new PagedResult<ApplicationUser>(items, total);
    }

    public async Task<bool> AddRole(ApplicationUser user, string role)
    {
        _logger.LogInformation("Adding role {Role} to user {UserId}", role, user.Id);
        var result = await _userManager.AddToRoleAsync(user, role);
        if (result.Succeeded)
        {
            _logger.LogInformation("Role {Role} added to user {UserId} successfully", role, user.Id);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add role {Role} to user {UserId}: {Errors}", role, user.Id, errors);
        }
        return result.Succeeded;
    }

    public async Task<ApplicationUser?> FindByEmail(string email)
    {
        _logger.LogInformation("Finding user by email {Email}", email);
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User with email {Email} not found", email);
            throw new NotFoundException(nameof(ApplicationUser), email);
        }
        return user;
    }

    public async Task<ApplicationUser?> FindById(string id)
    {
        _logger.LogInformation("Finding user by Id {UserId}", id);
        ApplicationUser? user = await _userManager.Users
            .Include(u => u.Address)
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }
        return user;
    }

    public async Task<bool> CheckPassword(ApplicationUser user, string password)
    {
        _logger.LogInformation("Checking password for user {UserId}", user.Id);
        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
            _logger.LogWarning("Password check failed for user {UserId}", user.Id);
        return result;
    }

    public async Task<bool> CheckUserIfExistByEmail(string email)
    {
        _logger.LogInformation("Checking if user exists by email {Email}", email);
        var result = await _userManager.FindByEmailAsync(email);
        var exists = result != null;
        _logger.LogInformation("User with email {Email} exists: {Exists}", email, exists);
        return exists;
    }

    public async Task<bool> DeleteUser(string id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);
        ApplicationUser? user = await FindById(id);
        if (user == null)
        {
            _logger.LogWarning("Delete user failed — user {UserId} not found", id);
            throw new NotFoundException(nameof(ApplicationUser), id);
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} deleted successfully", id);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to delete user {UserId}: {Errors}", id, errors);
        }
        return result.Succeeded;
    }
}