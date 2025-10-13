using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;
public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int? id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateAsync(User newUser);
    Task<bool> UpdateAsync(User user);
    Task<User?> DeleteAsync(int id);
    Task<List<User>?> GetAllUsersAsync();
}