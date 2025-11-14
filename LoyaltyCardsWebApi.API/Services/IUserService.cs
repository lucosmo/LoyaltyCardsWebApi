using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services;
public interface IUserService
{
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto newUser, CancellationToken cancellationToken = default);
    Task<Result<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByIdAsync(int? currentUserId, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByIdAsync(int userId, int? currentUserId, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByEmailAsync(string currentUserEmail, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateUserAsync(int id, UpdatedUserDto updatedUser, int currentUserId, CancellationToken cancellationToken = default);
}