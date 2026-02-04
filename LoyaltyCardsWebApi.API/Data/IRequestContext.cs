using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data;

public interface IRequestContext
{
    string? CorrelationId { get; }
    string? DeviceId { get; }
    string? GuestId { get; }
    string? IpAddress { get; }
    string? Locale { get; }
    string? Method { get; }
    string? SessionId { get; }
    string? SpanId { get; }
    string? TraceIdentifier { get; }
    string? TraceId { get; }
    string? UserAgent { get; }
    string? QueryParams { get; }
}