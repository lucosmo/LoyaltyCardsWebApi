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
    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto newUser)
    {
        if (newUser is null)
        {
            return Result<UserDto>.BadRequest("User data is required to create a new User.");
        }
        var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
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

        var createdUser = await _userRepository.CreateAsync(newUserModel);
        return Result<UserDto>.Ok(createdUser.ToDto());
    }

    private async Task<Result<UserDto>> GetUserByIdCoreAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }
        return Result<UserDto>.Ok(user.ToDto());
    } 
    public async Task<Result<UserDto>> GetUserByIdAsync(int? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }
        if (currentUserId.Value <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        return await GetUserByIdCoreAsync(currentUserId.Value);
    }
    public async Task<Result<UserDto>> GetUserByIdAsync(int userId, int? currentUserId)
    {
        if (!currentUserId.HasValue || userId != currentUserId.Value)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        return await GetUserByIdCoreAsync(currentUserId.Value);
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string currentUserEmail)
    {
        if (string.IsNullOrEmpty(currentUserEmail))
        {
            return Result<UserDto>.BadRequest("Invalid email.");    
        }

        var user = await _userRepository.GetUserByEmailAsync(currentUserEmail);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
      
    public async Task<Result<UserDto>> DeleteAsync(int userId, int currentUserId)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        if (userId != currentUserId)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.DeleteAsync(userId);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());

    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync() ?? new List<User>();
        var userDtos = users.Select(u => u.ToDto()).ToList();
        return Result<List<UserDto>>.Ok(userDtos);
    }

    public async Task<Result<bool>> UpdateUserAsync(int userId, UpdatedUserDto updatedUser, int currentUserId)
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
        var existingUser = await _userRepository.GetUserByIdAsync(userId); 
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

        var isUserUpdated = await _userRepository.UpdateAsync(existingUser);
        if (!isUserUpdated)
        {
            return Result<bool>.Fail("User update failed.");
        }

        return Result<bool>.Ok(true);
    }
}