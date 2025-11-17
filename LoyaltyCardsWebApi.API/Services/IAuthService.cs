using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;
public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> RegisterAsync(CreateUserDto newUserDto, CancellationToken cancellationToken = default);
    Result<string> GetTokenAuthHeader();
    DateTime? GetTokenExpiryDate();
    Result<int> GetUserId();
    Task<Result<string>> AddRevokedTokenAsync(string token, int userId, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);
    Task<Result<bool>> RevokeAllTokensForUserAsync(int userId, CancellationToken cancellationToken = default);
}