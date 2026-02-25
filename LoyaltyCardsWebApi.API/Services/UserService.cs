using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto newUser, CancellationToken cancellationToken = default)
    {
        if (newUser is null)
        {
            return Result<UserDto>.BadRequest("User data is required to create a new User.");
        }
        var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Admin create user failed: Email {UserEmail} already exists.", newUser.Email);
            return Result<UserDto>.Conflict($"User with this email: {newUser.Email} already exists.");
        }
        var newUserModel = new User
        {
            UserName = newUser.UserName,
            Email = newUser.Email,
            AccountCreatedDate = DateTime.UtcNow,
            Role = UserRole.User,
            PasswordHash = string.Empty
        };
        newUserModel.PasswordHash = _passwordHasher.HashPassword(newUserModel, newUser.Password);

        var createdUser = await _userRepository.CreateAsync(newUserModel, cancellationToken);
        _logger.LogInformation("User created by Admin. New UserId: {NewUserId}, Email: {UserEmail}", createdUser.Id, createdUser.Email);
        return Result<UserDto>.Ok(createdUser.ToDto());
    }

    private async Task<Result<UserDto>> GetUserByIdCoreAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }
        return Result<UserDto>.Ok(user.ToDto());
    } 
    
    public async Task<Result<UserDto>> GetCurrentUserAsync(int? currentUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUserId.HasValue)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }
        if (currentUserId.Value <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        return await GetUserByIdCoreAsync(currentUserId.Value, cancellationToken);
    }
    public async Task<Result<UserDto>> GetUserByIdAsync(int userId, int? currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }
        if (!currentUserId.HasValue || userId != currentUserId.Value)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        return await GetUserByIdCoreAsync(userId, cancellationToken);
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string currentUserEmail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return Result<UserDto>.BadRequest("Invalid email.");    
        }

        var user = await _userRepository.GetUserByEmailAsync(currentUserEmail, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
      
    public async Task<Result<UserDto>> DeleteAsync(int userId, int? currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        if (currentUserId is null)
        {
            return Result<UserDto>.Unauthorized("Authentication is required.");
        }

        if (userId != currentUserId)
        {
            _logger.LogWarning("Security Alert: User {CurrentUserId} tried to delete User {TargetUserId}", currentUserId, userId);
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.DeleteAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Delete failed: User {UserId} not found.", userId);
            return Result<UserDto>.NotFound("User not found.");
        }

        _logger.LogInformation("User {UserId} deleted their account.", userId);
        return Result<UserDto>.Ok(user.ToDto());

    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        var userDtos = users.Select(u => u.ToDto()).ToList();
        return Result<List<UserDto>>.Ok(userDtos);
    }

    public async Task<Result<bool>> UpdateUserAsync(int userId, UpdatedUserDto updatedUser, int? currentUserId, CancellationToken cancellationToken = default)
    {
        bool emailChanged = false;
        bool passChanged = false;

        if (userId <= 0)
        {
            return Result<bool>.BadRequest("Invalid user ID.");
        }

        if (currentUserId is null)
        {
            return Result<bool>.Unauthorized("Authentication is required.");
        }
            
        if (userId != currentUserId)
        {
            _logger.LogWarning("Security Alert: User {CurrentUserId} tried to update User {TargetUserId}", currentUserId, userId);
            return Result<bool>.Forbidden("No permission.");
        }

        if (updatedUser is null)
        {
            return Result<bool>.BadRequest("User data is required.");
        }
        var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken); 
        if (existingUser is null)
        {
            return Result<bool>.NotFound("User not found.");
        }

        string oldEmail = existingUser.Email;
        if (updatedUser.Email is not null && updatedUser.Email != existingUser.Email)
        {
            var existingEmail = await _userRepository.GetUserByEmailAsync(updatedUser.Email, cancellationToken);
            if (existingEmail is not null)
            {
                return Result<bool>.Conflict("Email is already in use by another account.");
            }
            existingUser.Email = updatedUser.Email;
            emailChanged = true;
        }
        
        if (!string.IsNullOrEmpty(existingUser.PasswordHash) && !string.IsNullOrEmpty(updatedUser.NewPassword))
        {
            var verifiedHashedCurrentPassword = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, updatedUser.CurrentPassword);
            if (verifiedHashedCurrentPassword == PasswordVerificationResult.Success || verifiedHashedCurrentPassword == PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (string.Equals(updatedUser.NewPassword, updatedUser.CurrentPassword, StringComparison.Ordinal))
                {
                    return Result<bool>.BadRequest("New password must be different from the current password.");
                }
                else
                {
                    existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, updatedUser.NewPassword);
                    passChanged = true;
                }
            }
            else
            {
                _logger.LogWarning("User {UserId} password change failed: Invalid current password.", userId);
                return Result<bool>.BadRequest("Invalid credentials.");
            }
        }

        var isUserUpdated = await _userRepository.UpdateAsync(cancellationToken);
        if (!isUserUpdated)
        {
            _logger.LogError("System Error: User {UserId} update failed in DB.", userId);
            return Result<bool>.Fail("User update failed.");
        }
        if (emailChanged)
        {
            _logger.LogInformation("User {UserId} changed email from {OldEmail} to {NewEmail}", userId, oldEmail, existingUser.Email);
        }
        if (passChanged)
        {
            _logger.LogInformation("User {UserId} changed password.", userId);
        }
        return Result<bool>.Ok(true);
    }
}