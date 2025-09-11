using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using System.Security.Claims;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Common;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
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