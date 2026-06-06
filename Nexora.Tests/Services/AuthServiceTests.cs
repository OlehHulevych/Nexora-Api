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
using Nexora.Domain.Enums;
using Nexora.Domain.Exceptions;

namespace Nexora.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAvatarService> _avatarServiceMock = null!;
    private Mock<IHttpContextAccessor> _httpAccessorMock = null!;
    private Mock<IJwtService> _jwtServiceMock = null!;
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<IAddressRepository> _addressRepoMock = null!;
    private Mock<ICartRepository> _cartRepoMock = null!;
    private Mock<IFavoriteListRepository> _favoriteListRepoMock = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private AuthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _avatarServiceMock = new Mock<IAvatarService>();
        _httpAccessorMock = new Mock<IHttpContextAccessor>();
        _jwtServiceMock = new Mock<IJwtService>();
        _userRepoMock = new Mock<IUserRepository>();
        _addressRepoMock = new Mock<IAddressRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _favoriteListRepoMock = new Mock<IFavoriteListRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();

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

    private static RegisterUserCommand BuildRegisterCommand(string confirmPassword = "Password123!") =>
        new(
            "john@test.com",
            "John",
            "Doe",
            "Password123!",
            confirmPassword,
            new Mock<IFormFile>().Object,
            new AddressCommand("123 St", null, "NY", "US", null)
        );

    // ── RegisterUserService ───────────────────────────────────────────

    [Test]
    public void RegisterUserService_ThrowsUserAlreadyExists_WhenEmailTaken()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(true);

        Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await _sut.RegisterUserService(BuildRegisterCommand()));
    }

    [Test]
    public void RegisterUserService_ThrowsPasswordNotMatched_WhenPasswordsMismatch()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<PasswordIsNotMatched>(async () =>
            await _sut.RegisterUserService(BuildRegisterCommand("WrongPassword")));
    }

    [Test]
    public void RegisterUserService_ThrowsBadHttpRequest_WhenUserCreateFails()
    {
        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddUser(
                It.IsAny<ApplicationUser>(), It.IsAny<string>(),
                It.IsAny<LoginProvider?>(), It.IsAny<Google.Apis.Auth.GoogleJsonWebSignature.Payload?>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.RegisterUserService(BuildRegisterCommand()));
    }

    [Test]
    public async Task RegisterUserService_Returns200_WhenSuccessful()
    {
        var avatar = new Avatar { Uri = "http://blob/avatar.jpg" };

        _userRepoMock.Setup(r => r.CheckUserIfExistByEmail(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddUser(
                It.IsAny<ApplicationUser>(), It.IsAny<string>(),
                It.IsAny<LoginProvider?>(), It.IsAny<Google.Apis.Auth.GoogleJsonWebSignature.Payload?>()))
            .ReturnsAsync(true);
        _addressRepoMock.Setup(r => r.AddAsync(It.IsAny<Address>())).ReturnsAsync(true);
        _userRepoMock.Setup(r => r.AddRole(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _avatarServiceMock.Setup(r => r.UploadAvatar(It.IsAny<UploadAvatarCommand>(), It.IsAny<string>()))
            .ReturnsAsync(new UploadAvatarResponse(avatar, "http://blob/avatar.jpg"));
        _cartRepoMock.Setup(r => r.Add(It.IsAny<Cart>())).ReturnsAsync(true);
        _favoriteListRepoMock.Setup(r => r.Add(It.IsAny<FavoriteList>())).ReturnsAsync(true);
        _userRepoMock.Setup(r => r.UpdateUser(It.IsAny<ApplicationUser>())).ReturnsAsync(true);

        var result = await _sut.RegisterUserService(BuildRegisterCommand());

        GetStatusCode(result).Should().Be(200);
    }

    // ── LoginUserHandler ──────────────────────────────────────────────

    [Test]
    public void LoginUserHandler_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<UserIsNotFoundException>(async () =>
            await _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "password123")));
    }

    [Test]
    public void LoginUserHandler_ThrowsPasswordNotMatched_WhenWrongPassword()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.CheckPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        Assert.ThrowsAsync<PasswordIsNotMatched>(async () =>
            await _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "wrong")));
    }

    [Test]
    public void LoginUserHandler_ThrowsBadHttpRequest_WhenTokenGenerationFails()
    {
        _userRepoMock.Setup(r => r.FindByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", Email = "test@test.com" });
        _userRepoMock.Setup(r => r.CheckPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _jwtServiceMock.Setup(j => j.CreateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((string?)null);

        Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _sut.LoginUserHandler(new LoginUserCommand("test@test.com", "password123")));
    }

    [Test]
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

    [Test]
    public void RetrieveUser_ThrowsUserIsNotFoundException_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.ThrowsAsync<UserIsNotFoundException>(async () =>
            await _sut.RetrieveUser("user-1"));
    }

    [Test]
    public async Task RetrieveUser_Returns200_WhenUserFound()
    {
        _userRepoMock.Setup(r => r.FindById(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = "user-1", FirstName = "John", LastName = "Doe",
                Email = "john@test.com",
                Avatar = new Avatar { Uri = "http://blob/avatar.jpg" },
                Address = new Address("user-1", "123 St", "NY", "US", null, null)
            });

        var result = await _sut.RetrieveUser("user-1");

        GetStatusCode(result).Should().Be(200);
    }
}
