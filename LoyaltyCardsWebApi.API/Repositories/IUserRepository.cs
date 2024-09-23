using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers();
    Task<User> GetUserByIdAsync(int id);
    Task<User> CreateAsync(User newUser);
    Task<User> Update(User user);
    Task<User> Delete(int id);
}