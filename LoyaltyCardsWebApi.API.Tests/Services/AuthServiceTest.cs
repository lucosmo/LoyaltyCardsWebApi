using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

[TestFixture]
public class AuthServiceTest
{
    private Mock<IUserContext> _userContext;
    private Mock<IUserRepository> _userRepository;
    private Mock<IJwtService> _jwtService;
    private Mock<IAuthRepository> _authRepository;
    private Mock<IRequestContext> _requestContext;
    private Mock<ICurrentUserService> _currentUserService;
    private Mock<ILogger<AuthService>> _logger;
    private IPasswordHasher<User> _passwordHasher;
    private AuthService _authService;
    [SetUp]
    public void SetUp()
    {
        _userContext = new Mock<IUserContext>();
        _requestContext = new Mock<IRequestContext>();
        _authRepository = new Mock<IAuthRepository>();
        _userRepository = new Mock<IUserRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _logger = new Mock<ILogger<AuthService>>();
        _jwtService = new Mock<IJwtService>();
        _passwordHasher = new PasswordHasher<User>();
        _authService = new AuthService(
            _userContext.Object,
            _authRepository.Object,
            _userRepository.Object,
            _currentUserService.Object,
            _jwtService.Object,
            _passwordHasher,
            _logger.Object
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

    [Test]
    public async Task LoginAsync_WhenJWTGenerationFailed_LogSystemErrorAndResultFailed()
    {
        // Arrange
        var passwordHashDefault = string.Empty;
        var email = "test@test.test";
        var correctPassword = "Test";
        var userName = "userTest";
        var userId = 1;
        var loginDto = new LoginDto { Email = email, Password = correctPassword };
        var user = new User { Id = userId, UserName = userName, Email = email, PasswordHash = passwordHashDefault, Role = 0 };
        user.PasswordHash = _passwordHasher.HashPassword(user, correctPassword);
        var token = string.Empty;

        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString())).Returns(token);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.That(result.Success, Is.False);
        _logger.Verify(
           x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, _) =>
                   v.ToString()!.Contains("System Error")),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
           Times.Once);
    }

    [Test]
    public async Task RegisterAsync_NewUserNotCreated_LogSystemErrorAndResultFailed()
    {
        // Arrange
        CreateUserDto newUser = new CreateUserDto
        {
            UserName = "userTest",
            Email = "test@test.test",
            Password = "password"
        };

        var user = new User
        {
            Id = 1,
            UserName = "userTest",
            Email = "test@test.test",
            PasswordHash = "password_hash",
            Role = UserRole.User
        };

        _userRepository.Setup(x => x.GetUserByEmailAsync(newUser.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.RegisterAsync(newUser, It.IsAny<CancellationToken>());

        // Assert
        Assert.That(result.Success, Is.False);
        _logger.Verify(
           x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, _) =>
                   v.ToString()!.Contains("System Error")),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
           Times.Once);
    }
}