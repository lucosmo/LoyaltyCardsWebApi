using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;
public interface IAuthRepository
{
    Task<RevokedToken> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId);
    Task<bool> IsTokenRevokedAsync(string token);
    Task RevokeAllTokensForUserAsync(int userId);
}