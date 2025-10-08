using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    public int? UserId { get; }
    public string? UserEmail { get; } 
}