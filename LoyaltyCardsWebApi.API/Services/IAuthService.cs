using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;
public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginDto loginDto);
    Task<Result<UserDto>> RegisterAsync(CreateUserDto newUserDto);
    Result<string> GetTokenAuthHeader();
    Result<DateTime> GetTokenExpiryDate();
    Result<int> GetUserId();
    Task<Result<string>> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId);
    Task<bool> IsTokenRevokedAsync(string token);

}