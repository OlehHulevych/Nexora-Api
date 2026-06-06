using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Common;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Application.Users.Services;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<IAvatarService> _avatarServiceMock = null!;
    private Mock<ILogger<UserService>> _loggerMock = null!;
    private UserService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _avatarServiceMock = new Mock<IAvatarService>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _sut = new UserService(
            _userRepoMock.Object,
            _avatarServiceMock.Object,
            _loggerMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;


    [Test]
    public void PromoteUserHandler_ThrowsBadHttpRequest_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.PromoteUserHandler("user-1"));
    }

    [Test]
    public void PromoteUserHandler_ThrowsBadHttpRequest_WhenRoleAssignFails()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.PromoteUserHandler("user-1"));
    }

    [Test]
    public async Task PromoteUserHandler_Returns200_WhenSuccessful()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = "user-1", Email = "test@test.com",
                FirstName = "John", LastName = "Doe"
            });
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _sut.PromoteUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void UpdateUserHandler_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<UserIsNotFoundException>(async () =>
            await _sut.UpdateUserHandler("user-1", new UpdateUserCommand(null, null, null, null, null)));
    }

    [Test]
    public async Task UpdateUserHandler_Returns200_WhenFieldsUpdated()
    {
        var user = new ApplicationUser
        {
            Id = "user-1", Email = "old@test.com",
            FirstName = "John", LastName = "Doe",
            Address = new Address("user-1", "123 St", "NY", "US", null, null)
        };
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>())).ReturnsAsync(user);

        var result = await _sut.UpdateUserHandler("user-1",
            new UpdateUserCommand("Jane", "Smith", null, "new@test.com", null));

        GetStatusCode(result).Should().Be(200);
        user.FirstName.Should().Be("Jane");
        user.Email.Should().Be("new@test.com");
    }

    [Test]
    public async Task UpdateUserHandler_Returns200_WithNullFields_KeepsOriginalValues()
    {
        var user = new ApplicationUser
        {
            Id = "user-1", Email = "old@test.com", FirstName = "John", LastName = "Doe"
        };
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>())).ReturnsAsync(user);

        var result = await _sut.UpdateUserHandler("user-1",
            new UpdateUserCommand(null, null, null, null, null));

        GetStatusCode(result).Should().Be(200);
        user.FirstName.Should().Be("John"); // unchanged
        user.Email.Should().Be("old@test.com"); // unchanged
    }


    [Test]
    public void BanUserHandler_ThrowsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.BanUserHandler("user-1"));
    }

    [Test]
    public async Task BanUserHandler_Returns200_AndSetsBannedTrue()
    {
        var user = new ApplicationUser { Id = "user-1", Banned = false };
        _userRepoMock.Setup(r => r.GetUser("user-1")).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateUser(It.IsAny<ApplicationUser>())).ReturnsAsync(true);

        var result = await _sut.BanUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
        user.Banned.Should().BeTrue();
    }


    [Test]
    public void UnBanUserHandler_ThrowsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _sut.UnBanUserHandler("user-1"));
    }

    [Test]
    public async Task UnBanUserHandler_Returns200_AndSetsBannedFalse()
    {
        var user = new ApplicationUser { Id = "user-1", Banned = true };
        _userRepoMock.Setup(r => r.GetUser("user-1")).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateUser(It.IsAny<ApplicationUser>())).ReturnsAsync(true);

        var result = await _sut.UnBanUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
        user.Banned.Should().BeFalse();
    }


    [Test]
    public void DeleteUserHandler_ThrowsBadHttpRequest_WhenDeleteFails()
    {
        _userRepoMock.Setup(r => r.DeleteUser(It.IsAny<string>())).ReturnsAsync(false);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.DeleteUserHandler("user-1"));
    }

    [Test]
    public async Task DeleteUserHandler_Returns200_WhenDeleted()
    {
        _userRepoMock.Setup(r => r.DeleteUser(It.IsAny<string>())).ReturnsAsync(true);

        var result = await _sut.DeleteUserHandler("user-1");

        GetStatusCode(result).Should().Be(200);
    }


    [Test]
    public void GetUsersHandler_ThrowsBadHttpRequest_WhenNoUsersFound()
    {
        _userRepoMock.Setup(r => r.GetAllUsers(It.IsAny<AllUserCommand>()))
            .ReturnsAsync(new PagedResult<ApplicationUser>([], 0));

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.GetUsersHandler(new AllUserCommand(null, 1)));
    }

    [Test]
    public async Task GetUsersHandler_Returns200_WhenUsersFound()
    {
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = "user-1", FirstName = "John", LastName = "Doe",
                Email = "john@test.com", CreatedAt = DateTime.UtcNow
            }
        };
        _userRepoMock.Setup(r => r.GetAllUsers(It.IsAny<AllUserCommand>()))
            .ReturnsAsync(new PagedResult<ApplicationUser>(users, 1));

        var result = await _sut.GetUsersHandler(new AllUserCommand(null, 1));

        GetStatusCode(result).Should().Be(200);
    }
}
