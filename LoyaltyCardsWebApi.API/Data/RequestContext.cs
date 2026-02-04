using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Linq;

namespace LoyaltyCardsWebApi.API.Data;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestContext> _logger;

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    public RequestContext(IHttpContextAccessor httpContextAccessor, ILogger<RequestContext> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? CorrelationId => 
        HttpContext?.Items.TryGetValue(ContextKeys.CorrelationIdKey, out var value) == true && value is string correlationId
        ? correlationId
        : null;
    public string? DeviceId =>
        HttpContext?.Items.TryGetValue(ContextKeys.DeviceIdKey, out var value) == true && value is string deviceId
        ? deviceId
        : null;
    public string? GuestId => 
        HttpContext?.Items.TryGetValue(ContextKeys.GuestIdKey, out var value) == true && value is string guestId
        ? guestId
        : null;
    public string? IpAddress =>
        HttpContext?.Items.TryGetValue(ContextKeys.ClientIpKey, out var value) == true && value is string clientIp
        ? clientIp
        : null;
    public string? Locale => HttpContext?.Request.Headers?.AcceptLanguage.FirstOrDefault();
    public string? Method => HttpContext?.Request.Method;
    public string? Path => HttpContext?.Request.Path.Value;
    public string? SessionId =>
        HttpContext?.Items.TryGetValue(ContextKeys.SessionIdKey, out var value) == true && value is string sessionId
        ? sessionId
        : null;
    public string? SpanId => Activity.Current?.SpanId.ToString();
    public string? TraceId => Activity.Current?.TraceId.ToString();
    public string? TraceIdentifier => HttpContext?.TraceIdentifier;
    public string? UserAgent =>
        HttpContext?.Items.TryGetValue(ContextKeys.UserAgentKey, out var value) == true && value is string userAgent
        ? userAgent
        : null;
    public string? QueryParams =>
        HttpContext?.Items.TryGetValue(ContextKeys.QueryParamsKey, out var value) == true && value is string queryParams
        ? queryParams
        : null;


}