using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Services;
using Nexora.Application.Common;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAvatarService> _avatarServiceMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepoMock.Object,
            _avatarServiceMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;


    [Fact]
    public async Task PromoteUserHandler_ThrowsBadHttpRequest_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.PromoteUserHandler("user-1"));
    }

    [Fact]
    public async Task PromoteUserHandler_ThrowsBadHttpRequest_WhenRoleAssignFails()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.PromoteUserHandler("user-1"));
    }

    [Fact]
    public async Task PromoteUserHandler_Returns200_WhenSuccessful()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com", FirstName = "John", LastName = "Doe" });
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _sut.PromoteUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Fact]
    public async Task UpdateUserHandler_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UserIsNotFoundException>(
            () => _sut.UpdateUserHandler("user-1", new UpdateUserCommand(null, null, null, null, null)));
    }

    [Fact]
    public async Task UpdateUserHandler_Returns200_WhenSuccessful()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = "user-1",
                Email = "old@test.com",
                FirstName = "John",
                LastName = "Doe",
                Address = new Address("user-1", "123 St", "NY", "US", null, null)
            });

        var result = await _sut.UpdateUserHandler("user-1",
            new UpdateUserCommand("Jane", null, null, "new@test.com", null));

        GetStatusCode(result).Should().Be(200);
    }

    
    [Fact]
    public async Task BanUserHandler_ThrowsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.BanUserHandler("user-1"));
    }

    [Fact]
    public async Task BanUserHandler_Returns200_WhenUserBanned()
    {
        var user = new ApplicationUser { Id = "user-1", Banned = false };
        _userRepoMock.Setup(r => r.GetUser("user-1")).ReturnsAsync(user);

        var result = await _sut.BanUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
        user.Banned.Should().BeTrue();
    }


    [Fact]
    public async Task UnBanUserHandler_ThrowsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UnBanUserHandler("user-1"));
    }

    [Fact]
    public async Task UnBanUserHandler_Returns200_WhenUserUnbanned()
    {
        var user = new ApplicationUser { Id = "user-1", Banned = true };
        _userRepoMock.Setup(r => r.GetUser("user-1")).ReturnsAsync(user);
        // no setup needed for UpdateUser

        var result = await _sut.UnBanUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
        user.Banned.Should().BeFalse();
    }


    [Fact]
    public async Task DeleteUserHandler_ThrowsBadHttpRequest_WhenDeleteFails()
    {
        _userRepoMock.Setup(r => r.DeleteUser(It.IsAny<string>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.DeleteUserHandler("user-1"));
    }

    [Fact]
    public async Task DeleteUserHandler_Returns200_WhenDeleted()
    {
        _userRepoMock.Setup(r => r.DeleteUser(It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.DeleteUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Fact]
    public async Task GetUsersHandler_ThrowsBadHttpRequest_WhenNoUsersFound()
    {
        _userRepoMock.Setup(r => r.GetAllUsers(It.IsAny<AllUserCommand>()))
            .ReturnsAsync(new PagedResult<ApplicationUser>([], 0));

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.GetUsersHandler(new AllUserCommand(null, 1)));
    }

    [Fact]
    public async Task GetUsersHandler_Returns200_WhenUsersFound()
    {
        var users = new List<ApplicationUser>
        {
            new() { Id = "user-1", FirstName = "John", LastName = "Doe", Email = "john@test.com", CreatedAt = DateTime.UtcNow }
        };
        _userRepoMock.Setup(r => r.GetAllUsers(It.IsAny<AllUserCommand>()))
            .ReturnsAsync(new PagedResult<ApplicationUser>(users, 1));

        var result = await _sut.GetUsersHandler(new AllUserCommand(null, 1));

        GetStatusCode(result).Should().Be(200);
    }
}