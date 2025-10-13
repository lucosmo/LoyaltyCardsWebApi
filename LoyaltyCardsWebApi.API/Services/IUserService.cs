using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services;
public interface IUserService
{
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto newUser);
    Task<Result<List<UserDto>>> GetAllUsersAsync();
    Task<Result<UserDto>> GetCurrentUserAsync();
    Task<Result<UserDto>> GetUserByIdAsync(int id, int currentUserId);
    Task<Result<UserDto>> GetUserByEmailAsync(string email, string currentUserEmail);
    Task<Result<UserDto>> DeleteAsync(int id, int currentUserId);
    Task<Result<bool>> UpdateUserAsync(int id, UpdatedUserDto updatedUser, int currentUserId);
}