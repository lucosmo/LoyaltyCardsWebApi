using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;
public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginDto loginDto);
    Task<Result<string>> RegisterAsync(CreateUserDto newUserDto);
    Result<string> GetTokenAuthHeader();
    Result<int> GetUserId();
    Result<DateTime> GetTokenExpiryDate();
    Task<Result<string>> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId);
    Task<bool> IsTokenRevokedAsync(string token);

}