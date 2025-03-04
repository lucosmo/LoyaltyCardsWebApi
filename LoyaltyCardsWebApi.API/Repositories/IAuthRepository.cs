using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface IAuthRepository
{
    Task<RevokedToken> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId);
    Task<bool> IsTokenRevokedAsync(string token);
    Task RevokeAllTokensForUserAsync(int userId);
}