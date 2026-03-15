using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Domain.Constants;
using Nexora.Domain.DTOs;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.GettingUsers;

public class GettingUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;

    public GettingUsersService(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
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