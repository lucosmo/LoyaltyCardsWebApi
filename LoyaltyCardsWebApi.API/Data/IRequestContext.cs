using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data;

public interface IRequestContext
{
    string? CorrelationId { get; }
    string? IpAddress { get; }
    string? Locale { get; }
    string? TraceIdentifier { get; }
    string? Authorization { get; }
    IEnumerable<Claim> Claims { get; }
    string? ExpiryTime { get; }
}