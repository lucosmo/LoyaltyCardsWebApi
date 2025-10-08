using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestContext> _logger;
    private const string CorrelationIdHeaderName = "Correlation-ID";
    private const string SubjectClaimType = "sub";
    private const string EmailClaimType = "email";
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext; 
    private int? _userId;
    private string? _userEmail;

    public RequestContext(IHttpContextAccessor httpContextAccessor, ILogger<RequestContext> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int? UserId => _userId ??= ParseId();
    public string? UserEmail => _userEmail ??= ParseEmail();
    public string? CorrelationId => HttpContext?.Request.Headers?[CorrelationIdHeaderName].FirstOrDefault();
    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? Locale => HttpContext?.Request.Headers?.AcceptLanguage.FirstOrDefault();
    public string? TraceIdentifier => HttpContext?.TraceIdentifier;
    public IEnumerable<Claim> Claims => HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;

    private int? ParseId()
    {
        if (HttpContext?.User is null)
        {
            return null;
        }
        var id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                 HttpContext.User.FindFirst(SubjectClaimType)?.Value;
        
        return int.TryParse(id, out int parsedId) ? parsedId : null;
    }

    private string? ParseEmail()
    {
        if (HttpContext?.User is null)
        {
            return null;
        }
        var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ??
                    HttpContext.User.FindFirst(EmailClaimType)?.Value;
        return email;
    }
}