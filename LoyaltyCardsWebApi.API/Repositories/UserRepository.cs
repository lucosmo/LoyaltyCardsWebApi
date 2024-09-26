using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<User> CreateAsync(User newUser)
    {
        var createdUser = await _appDbContext.Users.AddAsync(newUser);
        await _appDbContext.SaveChangesAsync();
        return createdUser.Entity;
    }

    public async Task<User?> DeleteAsync(int id)
    {
        User? userToDelete = await _appDbContext.Users.FindAsync(id);
        if (userToDelete != null)
        {
            _appDbContext.Users.Remove(userToDelete);
            await _appDbContext.SaveChangesAsync();
        }

        return userToDelete;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        User? user = await _appDbContext.Users.FindAsync(id);
        return user;
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