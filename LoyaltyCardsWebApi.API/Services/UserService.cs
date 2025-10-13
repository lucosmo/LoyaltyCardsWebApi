using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Common;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserService(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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

    public async Task<Result<UserDto>> GetCurrentUserAsync()
    {
        var userId = _currentUserService.UserId;
        
        if (userId is null)
        {
            return Result<UserDto>.Unauthorized("User ID not found.");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
    public async Task<Result<UserDto>> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }
        if (userId != _currentUserService.UserId)
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result<UserDto>.BadRequest("Invalid email.");    
        }

        if (!string.Equals(email, _currentUserService.UserEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Result<UserDto>.Forbidden("No permission.");
        }

        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user is null)
        {
            return Result<UserDto>.NotFound("User not found.");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
      
    public async Task<Result<UserDto>> DeleteAsync(int userId)
    {
        if (userId <= 0)
        {
            return Result<UserDto>.BadRequest("Invalid user ID.");
        }

        if (userId != _currentUserService.UserId)
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

    public async Task<Result<bool>> UpdateUserAsync(int userId, UpdatedUserDto updatedUser)
    {
        if (userId <= 0)
        {
            return Result<bool>.BadRequest("Invalid user ID.");
        }

        if (userId != _currentUserService.UserId)
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