using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Nexora.Application.Addresses;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Application.Users.Services;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAvatarService> _avatarServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpAccessorMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAddressRepository> _addressRepoMock = new();
    private readonly Mock<ICartRepository> _cartRepoMock = new();
    private readonly Mock<IFavoriteListRepository> _favoriteListRepoMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        _httpAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _sut = new AuthService(
            _avatarServiceMock.Object,
            _loggerMock.Object,
            _jwtServiceMock.Object,
            _userRepoMock.Object,
            _httpAccessorMock.Object,
            _addressRepoMock.Object,
            _cartRepoMock.Object,
            _favoriteListRepoMock.Object);
    }

    private static int GetStatusCode(IResult result) =>
        ((IStatusCodeHttpResult)result).StatusCode ?? 200;

    // ── RegisterUserService ───────────────────────────────────────────

    [Fact]
    public async Task RegisterUserService_ThrowsUserAlreadyExists_WhenEmailTaken()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<UserAlreadyExistsException>(
            () => _sut.RegisterUserService(BuildRegisterCommand()));
    }

    [Fact]
    public async Task RegisterUserService_ThrowsPasswordNotMatched_WhenPasswordsMismatch()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);

        var cmd = BuildRegisterCommand() with { ConfirmPassword = "wrong" };

        await Assert.ThrowsAsync<PasswordIsNotMatched>(
            () => _sut.RegisterUserService(cmd));
    }

    [Fact]
    public async Task RegisterUserService_ThrowsBadHttpRequest_WhenUserCreateFails()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddUser(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.RegisterUserService(BuildRegisterCommand()));
    }

    [Fact]
    public async Task RegisterUserService_Returns200_WhenSuccessful()
    {
        var avatar = new Avatar { Uri = "http://blob/avatar.jpg" };

        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddUser(It.IsAny<ApplicationUser>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>()))
            .ReturnsAsync(true);
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _avatarServiceMock.Setup(r => r.UploadAvatar(It.IsAny<UploadAvatarCommand>(), It.IsAny<string>()))
            .ReturnsAsync(new UploadAvatarResponse(avatar, "http://blob/avatar.jpg")); // ← Avatar first, uri second

        // AddAsync, Add, Add, UpdateUser all return Task — no setup needed, Moq handles automatically

        var result = await _sut.RegisterUserService(BuildRegisterCommand());

        GetStatusCode(result).Should().Be(200);
    }

    // ── LoginUserHandler ──────────────────────────────────────────────

    [Fact]
    public async Task LoginUserHandler_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UserIsNotFoundException>(
            () => _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "password123")));
    }

    [Fact]
    public async Task LoginUserHandler_ThrowsPasswordNotMatched_WhenWrongPassword()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.CheckPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<PasswordIsNotMatched>(
            () => _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "wrong")));
    }

    [Fact]
    public async Task LoginUserHandler_ThrowsBadHttpRequest_WhenTokenGenerationFails()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.CheckPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _jwtServiceMock.Setup(j => j.CreateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<BadHttpRequestException>(
            () => _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "password123")));
    }

    [Fact]
    public async Task LoginUserHandler_Returns200_WhenSuccessful()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.CheckPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _jwtServiceMock.Setup(j => j.CreateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("jwt-token-xyz");

        var result = await _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "password123"));

        GetStatusCode(result).Should().Be(200);
    }

    // ── RetrieveUser ──────────────────────────────────────────────────

    [Fact]
    public async Task RetrieveUser_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UserIsNotFoundException>(
            () => _sut.RetrieveUser("user-1"));
    }

    [Fact]
    public async Task RetrieveUser_Returns200_WhenUserFound()
    {
        _userRepoMock.Setup(r => r.FindById("user-1"))
            .ReturnsAsync(new ApplicationUser
            {
                Id = "user-1", FirstName = "John", LastName = "Doe",
                Email = "john@test.com", Avatar = new Avatar { Uri = "http://blob/img.jpg" },
                Address = new Address { Line1 = "123 Street", City = "NY", Country = "US" }
            });

        var result = await _sut.RetrieveUser("user-1");

        GetStatusCode(result).Should().Be(200);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static RegisterUserCommand BuildRegisterCommand() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@test.com",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        Address = new AddressCommand { Line1 = "123 St", City = "NY", Country = "US" },
        Avatar = new Mock<IFormFile>().Object
    };
}