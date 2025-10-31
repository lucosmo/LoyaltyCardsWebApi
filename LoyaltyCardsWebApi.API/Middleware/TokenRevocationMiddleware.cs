using System.Text;
using System.Text.Json;
using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Services;

namespace LoyaltyCardsWebApi.API.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRevocationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IProblemDetailsService problemDetailsService)
    {
        var token = authService.GetTokenAuthHeader();
        
        if (token.Success && !string.IsNullOrEmpty(token.Value))
        {
            var isRevoked = await authService.IsTokenRevokedAsync(token.Value);
            if (isRevoked)
            {
                var title = "Unauthorized";
                var statusCode = StatusCodes.Status401Unauthorized;
                var details = "Invalid token";
                var problemDetails = ProblemDetailsHelper.CreateProblemDetails(context, title, statusCode, details);
                var json = JsonSerializer.Serialize(problemDetails);
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(json, Encoding.UTF8);
                return;
            }
        }
        await _next(context);
    }
}