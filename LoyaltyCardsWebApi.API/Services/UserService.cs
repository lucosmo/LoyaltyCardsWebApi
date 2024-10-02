using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;

namespace LoyaltyCardsWebApi.API.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<User> CreateUserAsync(CreateUserDto newUser)
    {
        var newUserModel = new User
        {
            UserName = newUser.UserName,
            Email = newUser.Email,
            Password = newUser.Password,
            AccountCreatedDate = DateTime.UtcNow  
        };
        var createdUser = await _userRepository.CreateAsync(newUserModel);
        return createdUser;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return user;
    }
    public async Task<User?> DeleteAsync(int id)
    {
        var userToDelete = await _userRepository.DeleteAsync(id);       
        return userToDelete;
    }

    public async Task<List<User>?> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdatedUserDto updatedUser)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(id); 
        if (existingUser == null)
        {
            return false;
        }        
        if (updatedUser.Email != null && updatedUser.Email != existingUser.Email)
        {
            existingUser.Email = updatedUser.Email;
        }
        if (updatedUser.Password != null && updatedUser.Password != existingUser.Password)
        {
            existingUser.Password = updatedUser.Password;
        }

        var isUserUpdated = await _userRepository.UpdateAsync(existingUser);
        return isUserUpdated;
    }
}