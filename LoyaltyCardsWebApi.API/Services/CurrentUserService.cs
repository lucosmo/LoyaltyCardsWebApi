using System.Security.Claims;
using LoyaltyCardsWebApi.API.Data;

namespace LoyaltyCardsWebApi.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;
    private readonly IRequestContext _requestContext;
    private const string SubjectClaimType = "sub";
    private const string EmailClaimType = "email";
    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
    private readonly Lazy<int?> _userId;
    private readonly Lazy<string?> _userEmail;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger, IRequestContext requestContext)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));


        _userId = new Lazy<int?>(() =>
        {
            var id = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     HttpContext?.User.FindFirst(SubjectClaimType)?.Value;
            var parseResult = int.TryParse(id, out int parsedId);
            if (parseResult)
            {
                _logger.LogInformation(
                    "UserId Lazy eval successful; raw={Raw}, parsed={parseResult}, correlationId = {CorrelationId}, trace = {Trace}",
                    id,
                    parseResult,
                    _requestContext.CorrelationId,
                    _requestContext.TraceIdentifier
                );
                return parsedId;
            }
            else
            {
                _logger.LogInformation(
                    "UserId Lazy eval failed; parsed={parseResult}, correlationId = {CorrelationId}, trace = {Trace}",
                    parseResult,
                    _requestContext.CorrelationId,
                    _requestContext.TraceIdentifier
                );
                return null;
            }
        });

        _userEmail = new Lazy<string?>(() =>
        {
            var email = HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
                        HttpContext?.User.FindFirst(EmailClaimType)?.Value;

            _logger.LogInformation(
                "UserEmail Lazy eval successful; raw={Raw}, correlationId = {CorrelationId}, trace = {Trace}",
                email,
                _requestContext.CorrelationId,
                _requestContext.TraceIdentifier
            );
            return email;
        });
    }

    public int? UserId => _userId.Value;
    public string? UserEmail => _userEmail.Value;
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;
}