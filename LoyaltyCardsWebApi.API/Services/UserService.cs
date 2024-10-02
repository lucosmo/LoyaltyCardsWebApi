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
}