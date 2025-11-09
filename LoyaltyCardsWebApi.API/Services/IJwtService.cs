namespace LoyaltyCardsWebApi.API.Services;

public interface IJwtService
{
    public string GenerateToken(string userId, string userEmail, string userRole);
}