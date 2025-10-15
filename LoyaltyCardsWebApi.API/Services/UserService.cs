using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Common;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto newUser)
    {
        if (newUser is null)
        {
            return Result<UserDto>.BadRequest("User data is required to create a new User.");
        }
        var newUserModel = new User
        {
            UserName = newUser.UserName,
            Email = newUser.Email,
            Password = newUser.Password,
            AccountCreatedDate = DateTime.UtcNow  
        };
        var createdUser = await _userRepository.CreateAsync(newUserModel);
        return Result<UserDto>.Ok(createdUser.ToDto());
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(int? currentUserId)
    {
        if (currentUserId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }
        if (currentUserId is null)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.GetUserByIdAsync(currentUserId.Value);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
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
        if (updatedUser.Password != null && updatedUser.Password != existingUser.Password)
        {
            existingUser.Password = updatedUser.Password;
        }

        var isUserUpdated = await _userRepository.UpdateAsync(existingUser);
        if (!isUserUpdated)
        {
            return Result<bool>.Fail("User update failed.");
        }

        return Result<bool>.Ok(true);
    }
}