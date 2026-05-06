using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Common;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Domain.Constants;
using Nexora.Domain.Entities;
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class UserRepository:IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<bool> AddUser(ApplicationUser user, string? password, LoginProvider? loginProvider, GoogleJsonWebSignature.Payload? payload)
    {
        IdentityResult? result = null;
        if (password != null)
        {
            result =await  _userManager.CreateAsync(user, password);
           
        }

        if (password == null && loginProvider == LoginProvider.Google && payload != null )
        {
            result = await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, new UserLoginInfo(
                LoginNames.Google,
                payload.Subject,
                LoginNames.Google));
        }
        
        if(result is not null) return result.Succeeded;
        return false;
    }

    public async Task<bool> UpdateUser(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<ApplicationUser?> GetUser(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), id);
        return user;
    }

    public async Task<PagedResult<ApplicationUser>> GetAllUsers(AllUserCommand request)
    {
        IQueryable<ApplicationUser> query = _userManager.Users.
            Include(u=>u.Address).
            Include(u=>u.Avatar)
            .AsQueryable();
        if (!String.IsNullOrEmpty(request.role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.role);
            var ids = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => ids.Contains(u.Id));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.CreatedAt).Skip((request.currentPage - 1) * 10)
            .Take(10).ToListAsync();
        return new PagedResult<ApplicationUser>(items,total);

    }

    public async Task<bool> AddRole(ApplicationUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<ApplicationUser?> FindByEmail(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), email);
        return user;
    }
    public async Task<ApplicationUser?> FindById(string email)
    {
        ApplicationUser? user = await _userManager.Users
            .Include(u=>u.Address)
            .Include(u=>u.Avatar).FirstOrDefaultAsync(u=>u.Email==email);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), email);
        return user;
    }

    public async Task<bool> CheckPassword(ApplicationUser user, string password)
    {
        var result = await _userManager.CheckPasswordAsync(user, password);
        return result;
    }

    public async Task<bool> DeleteUser(string id)
    {
        ApplicationUser? user = await FindById(id);
        if (user == null) throw new NotFoundException(nameof(ApplicationUser), id);
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}