using System.Text;
using System.Text.Json;
using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Services;

namespace LoyaltyCardsWebApi.API.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRevocationMiddleware> _logger;

    public TokenRevocationMiddleware(RequestDelegate next, ILogger<TokenRevocationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IProblemDetailsService problemDetailsService)
    {
        var token = authService.GetTokenAuthHeader();
        
        if (token.Success && !string.IsNullOrEmpty(token.Value))
        {
            var isRevoked = await authService.IsTokenRevokedAsync(token.Value, context.RequestAborted);
            if (isRevoked)
            {
                _logger.LogWarning("Security Alert: Request denied. Token has been revoked. IP: {ClientIp}", context.Connection.RemoteIpAddress?.ToString());
                var title = "Unauthorized";
                var statusCode = StatusCodes.Status401Unauthorized;
                var details = "Invalid token";
                var problemDetails = ProblemDetailsHelper.CreateProblemDetails(context, title, statusCode, details);
                var json = JsonSerializer.Serialize(problemDetails);
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                await context.Response.WriteAsync(json, Encoding.UTF8, context.RequestAborted);
                return;
            }
        }
        await _next(context);
    }
}