using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateAsync(User newUser);
    Task<bool> UpdateAsync(User user);
    Task<User?> DeleteAsync(int id);
    Task<List<User>?> GetAllUsersAsync();
}