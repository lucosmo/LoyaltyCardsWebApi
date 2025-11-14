using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

[TestFixture]
public class AuthServiceTest
{
    private Mock<IUserRepository> _userRepository;
    private Mock<IJwtService> _jwtService;
    private Mock<IAuthRepository> _authRepository;
    private Mock<IRequestContext> _requestContext;
    private Mock<ICurrentUserService> _currentUserService;
    private IPasswordHasher<User> _passwordHasher;
    private AuthService _authService;
    [SetUp]
    public void SetUp()
    {
        _currentUserService = new Mock<ICurrentUserService>();
        _requestContext = new Mock<IRequestContext>();
        _authRepository = new Mock<IAuthRepository>();
        _userRepository = new Mock<IUserRepository>();
        _jwtService = new Mock<IJwtService>();
        _passwordHasher = new PasswordHasher<User>();
        _authService = new AuthService(
            _requestContext.Object,
            _authRepository.Object,
            _userRepository.Object,
            _currentUserService.Object,
            _jwtService.Object,
            _passwordHasher
            );
    }

    [Test]
    public async Task LoginAsync_ProperInput_ReturnsToken()
    {
        var passwordHashDefault = string.Empty;
        var email = "test@test.test";
        var correctPassword = "Test";
        var userName = "userTest";
        var userId = 1;
        var loginDto = new LoginDto { Email = email, Password = correctPassword };
        var user = new User { Id = userId, UserName = userName, Email = email, PasswordHash = passwordHashDefault, Role = 0 };
        user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);
        var token = "thisIsTestToken";

        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString())).Returns(token);

        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.True);
        Assert.That(token, Is.EqualTo(result.Value));
    }

    [Test]
    public void LoginAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        var passwordHashDefault = string.Empty;
        var email = "test@test.test";
        var correctPassword = "Test";
        var userName = "userTest";
        var userId = 1;
        var loginDto = new LoginDto{ Email = email, Password = correctPassword };
        var user = new User { Id = userId, UserName = userName, Email = email, PasswordHash = passwordHashDefault, Role = 0 };
        user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);
        var token = "thisIsTestToken";
        using var cts = new CancellationTokenSource();

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString())).Returns(token);

        Exception ex = Assert.CatchAsync(async () =>
        {
            await _authService.LoginAsync(loginDto, cts.Token);
        });

        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _jwtService.Verify(js => js.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString()), Times.Never);
    }

    [Test]
    public async Task LoginAsync_WrongCredentials_ReturnsEmpty()
    {
        var passwordHashDefault = string.Empty;
        var email = "test@test.test";
        var correctPassword = "Test";
        var incorrectPassword = "WrongPassword";
        var userName = "userTest";
        var userId = 1;
        var loginDto = new LoginDto{ Email = email, Password = incorrectPassword };
        var user = new User { Id = userId, UserName = userName, Email = email, PasswordHash = passwordHashDefault, Role = 0 };
        user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);

        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
                     
        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid credentials."));
    }

    [Test]
    public async Task LoginAsync_NoUserFound_ReturnsEmpty()
    {
        var loginDto = new LoginDto{ Email = "wrongtest@test.test", Password = "Test" };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
                     
        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid credentials."));
    }

    [Test]
    public async Task LoginAsync_ServicesCalledOnce_TestPass()
    {
        // Arrange
        var passwordHashDefault = string.Empty;
        var email = "test@test.test";
        var correctPassword = "Test";
        var userName = "userTest";
        var userId = 1;
        var loginDto = new LoginDto{ Email = email, Password = correctPassword };
        var user = new User { Id = userId, UserName = userName, Email = email, PasswordHash = passwordHashDefault, Role = 0 };
        user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);
        var token = "thisIsTestToken";
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString())).Returns(token);
                     
        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()), Times.Once);
        _jwtService.Verify(x => x.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString()), Times.Once);
    }
}