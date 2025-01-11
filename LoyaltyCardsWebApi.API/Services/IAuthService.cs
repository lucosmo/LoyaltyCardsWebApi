using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;
public interface IAuthService
{
    Task<string> LoginAsync(LoginDto loginDto);
}