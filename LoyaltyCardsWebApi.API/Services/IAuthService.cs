using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;
public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginDto loginDto);
    Task<Result<UserDto>> RegisterAsync(CreateUserDto newUserDto);
    Result<string> GetTokenAuthHeader();
    DateTime? GetTokenExpiryDate();
    Result<int> GetUserId();
    Task<Result<string>> AddRevokedTokenAsync(string token, int userId);
    Task<bool> IsTokenRevokedAsync(string token);

}