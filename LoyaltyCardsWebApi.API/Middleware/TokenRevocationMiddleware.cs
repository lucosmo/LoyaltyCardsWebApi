using LoyaltyCardsWebApi.API.Services;

namespace LoyalityCardsWebApi.API.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRevocationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        string? token = authService.GetTokenAuthHeader().Value;
        if (!string.IsNullOrEmpty(token))
        {
            var isRevoked = await authService.IsTokenRevokedAsync(token);
            if (isRevoked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Please log in again.");
                return;
            }
        }
                
        await _next(context);
    }
}