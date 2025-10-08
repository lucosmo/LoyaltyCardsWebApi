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
    private readonly Lazy<int?> _userId;
    private readonly Lazy<string?> _userEmail;

    public RequestContext(IHttpContextAccessor httpContextAccessor, ILogger<RequestContext> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _userId = new Lazy<int?>(() =>
        {
            var id = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     HttpContext?.User.FindFirst(SubjectClaimType)?.Value;
            var parseResult = int.TryParse(id, out int parsedId);
            if (parseResult)
            {
                _logger.LogInformation(
                    "UserId Lazy eval successful; raw={Raw}, parsed={parseResult}, correlationId = {CorrelatioId}, trace = {Trace}",
                    id,
                    parseResult,
                    CorrelationId,
                    TraceIdentifier
                );
                return parsedId;
            }
            else
            {
                _logger.LogInformation(
                    "UserId Lazy eval failed; parsed={parseResult}, correlationId = {CorrelatioId}, trace = {Trace}",
                    parseResult,
                    CorrelationId,
                    TraceIdentifier
                );
                return null;
            }
        });

        _userEmail = new Lazy<string?>(() =>
        {
            var email = HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
                        HttpContext?.User.FindFirst(EmailClaimType)?.Value;

            _logger.LogInformation(
                "UserEmail Lazy eval successful; raw={Raw}, correlationId = {CorrelatioId}, trace = {Trace}",
                email,
                CorrelationId,
                TraceIdentifier
            );
            return email;
        });
    }

    public int? UserId => _userId.Value;
    public string? UserEmail => _userEmail.Value;
    public string? CorrelationId => HttpContext?.Request.Headers?[CorrelationIdHeaderName].FirstOrDefault();
    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? Locale => HttpContext?.Request.Headers?.AcceptLanguage.FirstOrDefault();
    public string? TraceIdentifier => HttpContext?.TraceIdentifier;
    public IEnumerable<Claim> Claims => HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;
}