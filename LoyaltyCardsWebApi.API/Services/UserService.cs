using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using System.Security.Claims;
using LoyaltyCardsWebApi.API.Extensions;

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
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Result<UserDto>.Fail("User ID not found");
        }

        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return Result<UserDto>.Fail("User not found");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
    public async Task<Result<UserDto>> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);

        if (user == null)
        {
            return Result<UserDto>.Fail("User not found");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        
        if (user == null)
        {
            return Result<UserDto>.Fail("User not found");
        }

        return Result<UserDto>.Ok(user.ToDto());
    }
      
    public async Task<Result<UserDto>> DeleteAsync(int id)
    {
        var user = await _userRepository.DeleteAsync(id);

        if (user == null)
        {
            return Result<UserDto>.Fail("User not found");
        }

        return Result<UserDto>.Ok(user.ToDto());

    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync() ?? new List<User>();
        var userDtos = users.Select(u => u.ToDto()).ToList();
        return Result<List<UserDto>>.Ok(userDtos);
    }

    public async Task<Result<bool>> UpdateUserAsync(int id, UpdatedUserDto updatedUser)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(id); 
        if (existingUser == null || updatedUser == null)
        {
            return Result<bool>.Fail("");
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
        return Result<bool>.Ok(isUserUpdated);
    }
}