using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers();
    Task<User> GetUserById(int id);
    Task<User> Create(User newUser);
    Task<User> Update(User user);
    Task<User> Delete(int id);
}