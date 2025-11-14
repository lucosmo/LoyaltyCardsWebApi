using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyCardsWebApi.API.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<User> CreateAsync(User newUser, CancellationToken cancellationToken = default)
    {
        var createdUser = await _appDbContext.Users.AddAsync(newUser, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return createdUser.Entity;
    }

    public async Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        User? userToDelete = await _appDbContext.Users.FindAsync(id, cancellationToken);
        if (userToDelete != null)
        {
            _appDbContext.Users.Remove(userToDelete);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        return userToDelete;
    }

    public async Task<List<User>?> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _appDbContext.Users.ToListAsync(cancellationToken);
        return users;
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await _appDbContext.Users.FindAsync(id, cancellationToken);
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        User? user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email.Equals(email), cancellationToken);
        return user;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _appDbContext.Users.Update(user);
        var changedRows = await _appDbContext.SaveChangesAsync(cancellationToken);
        return changedRows > 0;
    }
}