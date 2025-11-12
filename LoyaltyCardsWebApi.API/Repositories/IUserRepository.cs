using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;
public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User newUser, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<User>?> GetAllUsersAsync(CancellationToken cancellationToken = default);
}