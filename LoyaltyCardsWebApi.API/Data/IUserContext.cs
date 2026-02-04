using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data
{
    public interface IUserContext
    {
        string? AuthorizationHeader { get; }
        bool IsAuthenticated { get; }
        string? UserId { get; }
        DateTimeOffset? TokenExpiryTime { get; }
        string? Role { get; }
        string? OrganizationId { get; }
        string? SubscriptionTier { get; }
        string? Country { get; }
        string? Timezone { get; }
        int? AccountAgeDays { get; }
        IEnumerable<Claim> Claims { get; }
    }
}
