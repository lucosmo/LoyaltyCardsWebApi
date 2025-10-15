using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestContext> _logger;
    private const string CorrelationIdHeaderName = "Correlation-ID";
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    public RequestContext(IHttpContextAccessor httpContextAccessor, ILogger<RequestContext> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? Authorization => HttpContext?.Request.Headers?.Authorization.ToString();
    public string? CorrelationId => HttpContext?.Request.Headers?[CorrelationIdHeaderName].FirstOrDefault();
    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? Locale => HttpContext?.Request.Headers?.AcceptLanguage.FirstOrDefault();
    public string? TraceIdentifier => HttpContext?.TraceIdentifier;
    public IEnumerable<Claim> Claims => HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    public string? ExpiryTime => HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
}