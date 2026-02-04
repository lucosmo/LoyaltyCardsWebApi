using System.Security.Claims;

namespace LoyaltyCardsWebApi.API.Data;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestContext> _logger;

    private const string SubjectClaimType = "sub";
    private const string OrganizationClaimType = "organization_id";
    private const string SubscriptionClaimType = "subscription_tier";
    private const string CountryClaimType = "country";
    private const string TimeZoneClaimType = "time_zone";
    private const string AccountAgeClaimType = "account_age_days";
    
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    public UserContext(IHttpContextAccessor httpContextAccessor, ILogger<RequestContext> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;
    public string? UserId => HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     HttpContext?.User.FindFirst(SubjectClaimType)?.Value;
    public string? AuthorizationHeader => HttpContext?.Request?.Headers.Authorization;

    public DateTimeOffset? TokenExpiryTime => 
        HttpContext?.Items.TryGetValue(ContextKeys.TokenExpTime, out var value) == true && value is DateTimeOffset date
        ? date
        : null;

    public string? Role => HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    public string? OrganizationId => HttpContext?.User.FindFirst(OrganizationClaimType)?.Value;

    public string? SubscriptionTier => HttpContext?.User.FindFirst(SubscriptionClaimType)?.Value;

    public string? Country => HttpContext?.User.FindFirst(ClaimTypes.Country)?.Value ?? HttpContext?.User.FindFirst(CountryClaimType)?.Value;

    public string? Timezone => HttpContext?.User.FindFirst(TimeZoneClaimType)?.Value;

    public int? AccountAgeDays
    {
        get
        {
            var value = HttpContext?.User.FindFirst(AccountAgeClaimType)?.Value;
            return int.TryParse(value, out var days) ? days : null;
        }
    }

    public IEnumerable<Claim> Claims => HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
}

