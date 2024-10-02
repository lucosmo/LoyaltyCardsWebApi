using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services;
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto newUser);
    Task<List<User>?> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> DeleteAsync(int id);
    Task<bool> UpdateUserAsync(int id, UpdatedUserDto updatedUser);
}