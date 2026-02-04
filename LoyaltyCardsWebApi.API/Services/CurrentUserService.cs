using System.Security.Claims;
using LoyaltyCardsWebApi.API.Data;

namespace LoyaltyCardsWebApi.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;
    private const string SubjectClaimType = "sub";
    private const string EmailClaimType = "email";
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    private readonly Lazy<int?> _userId;
    private readonly Lazy<string?> _userEmail;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _userId = new Lazy<int?>(() =>
        {
            if (IsAuthenticated != true)
            {
                return null;
            }

            var id = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     HttpContext?.User.FindFirst(SubjectClaimType)?.Value;
            var parseResult = int.TryParse(id, out int parsedId);
            if (parseResult)
            {
                _logger.LogDebug("UserId Lazy eval successful; raw={Raw}, parsed={parseResult}",id, parseResult);
                return parsedId;
            }
            else
            {
                _logger.LogDebug("UserId Lazy eval failed; raw={Raw}, parsed={parseResult}", id, parseResult);
                return null;
            }
        });

        _userEmail = new Lazy<string?>(() =>
        {
            if (IsAuthenticated != true)
            {
                return null;
            }
            
            var email = HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
                        HttpContext?.User.FindFirst(EmailClaimType)?.Value;

            _logger.LogDebug("UserEmail Lazy eval successful; raw={Raw}", email);
            return email;
        });
    }

    public int? UserId => _userId.Value;
    public string? UserEmail => _userEmail.Value;
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;
}