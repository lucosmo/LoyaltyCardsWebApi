using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public Task<User> Create(User newUser)
    {
        throw new NotImplementedException();
    }

    public Task<User> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetUsers()
    {
        throw new NotImplementedException();
    }

    public Task<User> Update(User user)
    {
        throw new NotImplementedException();
    }
}