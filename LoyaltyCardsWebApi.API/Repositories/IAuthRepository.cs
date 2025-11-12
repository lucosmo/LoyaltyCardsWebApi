using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;
public interface IAuthRepository
{
    Task<RevokedToken> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllTokensForUserAsync(int userId, CancellationToken cancellationToken = default);
}