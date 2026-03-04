using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

[TestFixture]
public class UserServiceTest
{
    private Mock<IUserRepository> _userRepository;
    private Mock<IPasswordHasher<User>> _passwordHasher;
    private Mock<ILogger<UserService>> _logger;
    private UserService _userService;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher<User>>();
        _logger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_userRepository.Object, _passwordHasher.Object, _logger.Object);
    }

    [Test]
    public void CreateUser_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        var newUserDto = new CreateUserDto
        {
            UserName = "Adam12",
            Email = "adam12@ggg12.uk",
            Password = "password12"
        };
        using var cts = new CancellationTokenSource();
        _userRepository
            .Setup(ur => ur.GetUserByEmailAsync(newUserDto.Email, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.CreateUserAsync(newUserDto, cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _userRepository.Verify(ur => ur.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void GetUserByIdAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        var id = 1;
        var currentId = 1;
        _userRepository
            .Setup(ur => ur.GetUserByIdAsync(id, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.GetUserByIdAsync(id, currentId, cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _userRepository.Verify(ur => ur.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetUserByEmailAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        var email = "adam12@ggg12.uk";
        _userRepository
            .Setup(ur => ur.GetUserByEmailAsync(email, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.GetUserByEmailAsync(email, cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _userRepository.Verify(ur => ur.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void DeleteAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        var id = 1;
        var currentId = 1;
        _userRepository
            .Setup(ur => ur.DeleteAsync(id, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.DeleteAsync(id, currentId, cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _userRepository.Verify(ur => ur.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetAllUsersAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        _userRepository
            .Setup(ur => ur.GetAllUsersAsync(It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.GetAllUsersAsync(cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _userRepository.Verify(ur => ur.GetAllUsersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void UpdateUserAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        var id = 1;
        var currentId = 1;
        var updatedUser = new UpdatedUserDto
        {
            Email = "adam12@ggg12.uk",
            CurrentPassword = "password12",
            NewPassword = "newPassword32##"
        };
        _userRepository
            .Setup(ur => ur.GetUserByIdAsync(id, It.Is<CancellationToken>(ct => ct == cts.Token)))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        //Act
        Exception ex = Assert.CatchAsync(async () =>
        {
            await _userService.UpdateUserAsync(id, updatedUser, currentId, cts.Token);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _passwordHasher.Verify(ph => ph.HashPassword(It.IsAny<User>(),It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_WhenUserTriesDeleteAnotherUser_LogSecurityAlertWarning()
    {
        var userId = 1;
        var currentUserId = 2;

        var result = await _userService.DeleteAsync(userId, currentUserId);
        _logger.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) =>
                v.ToString()!.Contains("Security Alert")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenUserTriesUpdateAnotherUser_LogSecurityAlertWarning()
    {
        var userId = 1;
        var currentUserId = 2;
        var updatedUser = new UpdatedUserDto
        {
            Email = "adam12@ggg12.uk",
            CurrentPassword = "password12",
            NewPassword = "newPassword32##"
        };

        var result = await _userService.UpdateUserAsync(userId, updatedUser, currentUserId);
        _logger.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) =>
                v.ToString()!.Contains("Security Alert")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }
}