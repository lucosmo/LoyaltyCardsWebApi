using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Identity;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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
        return Result<UserDto>.Ok(createdUser.ToDto());
    }

    private async Task<Result<UserDto>> GetUserByIdCoreAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }
        return Result<UserDto>.Ok(user.ToDto());
    } 
    
    public async Task<Result<UserDto>> GetUserByIdAsync(int? currentUserId, CancellationToken cancellationToken = default)
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
        if (!currentUserId.HasValue || userId != currentUserId.Value)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        return await GetUserByIdCoreAsync(currentUserId.Value, cancellationToken);
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
      
    public async Task<Result<UserDto>> DeleteAsync(int userId, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        if (userId != currentUserId)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.DeleteAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());

    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllUsersAsync(cancellationToken) ?? new List<User>();
        var userDtos = users.Select(u => u.ToDto()).ToList();
        return Result<List<UserDto>>.Ok(userDtos);
    }

    public async Task<Result<bool>> UpdateUserAsync(int userId, UpdatedUserDto updatedUser, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<bool>.BadRequest("Invalid user ID.");
        }

        if (userId != currentUserId)
        {
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
        if (updatedUser.Email != null && updatedUser.Email != existingUser.Email)
        {
            existingUser.Email = updatedUser.Email;
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
                }
            }
            else
            {
                return Result<bool>.BadRequest("Invalid credentials.");
            }
        }

        var isUserUpdated = await _userRepository.UpdateAsync(existingUser, cancellationToken);
        if (!isUserUpdated)
        {
            return Result<bool>.Fail("User update failed.");
        }

        return Result<bool>.Ok(true);
    }
}