using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

[TestFixture]
public class AuthServiceTest
{
    private Mock<IUserRepository> _userRepository;
    private Mock<IJwtService> _jwtService;
    private Mock<IAuthRepository> _authRepository;
    private Mock<IHttpContextAccessor> _httpContextAccessor;
    private AuthService _authService;
    [SetUp]
    public void SetUp()
    {
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _authRepository = new Mock<IAuthRepository>();
        _userRepository = new Mock<IUserRepository>();
        _jwtService = new Mock<IJwtService>();
        _authService = new AuthService(_httpContextAccessor.Object, _authRepository.Object, _userRepository.Object, _jwtService.Object);
    }

    [Test]
    public async Task LoginAsync_ProperInput_ReturnsToken()
    {
        var loginDto = new LoginDto{ Email = "test@test.test", Password = "Test" };
        var user = new User{ Id = 1, UserName = "userTest", Email = "test@test.test", Password = "Test" };
        var token = "thisIsTestToken";
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email)).Returns(token);
        
        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.True);
        Assert.That(token, Is.EqualTo(result.Value));
    }

    [Test]
    public async Task LoginAsync_WrongCredentials_ReturnsEmpty()
    {
        var loginDto = new LoginDto{ Email = "test@test.test", Password = "WrongTest" };
        var user = new User{ Id = 1, UserName = "userTest", Email = "test@test.test", Password = "Test" };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);
             
        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid credentials"));
    }

    [Test]
    public async Task LoginAsync_NoUserFound_ReturnsEmpty()
    {
        var loginDto = new LoginDto{ Email = "wrongtest@test.test", Password = "Test" };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);
             
        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid credentials"));
    }

    [Test]
    public async Task LoginAsync_ServicesCalledOnce_TestPass()
    {
        // Arrange
        var loginDto = new LoginDto{ Email = "test@test.test", Password = "Test" };
        var user = new User{ Id = 1, UserName = "userTest", Email = "test@test.test", Password = "Test" };
        var token = "thisIsTestToken";
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _jwtService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email)).Returns(token);
        
        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginDto.Email), Times.Once);
        _jwtService.Verify(x => x.GenerateToken(user.Id.ToString(), user.Email), Times.Once);
    }
}