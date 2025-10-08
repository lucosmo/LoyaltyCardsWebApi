using System.Security.Claims;
using LoyaltyCardsWebApi.API.Services;

namespace LoyaltyCardsWebApi.API.Data;

public interface IRequestContext : ICurrentUserService
{
    string? CorrelationId { get; }
    string? IpAddress { get; }
    string? Locale { get; }
    string? TraceIdentifier { get; }
    IEnumerable<Claim> Claims { get; }
}