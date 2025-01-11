using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<User>?> GetAllUsersAsync()
    {
        var users = await _appDbContext.Users.ToListAsync();
        return users;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        User? user = await _appDbContext.Users.FindAsync(id);
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(LoginDto loginDto)
    {
        User? user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email.Equals(loginDto.Email));
        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        _appDbContext.Users.Update(user);
        var changedRows = await _appDbContext.SaveChangesAsync();
        return changedRows > 0;
    }
}