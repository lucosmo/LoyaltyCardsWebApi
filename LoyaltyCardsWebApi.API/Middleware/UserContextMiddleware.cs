using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.Services;
using Serilog.Context;
using System.IdentityModel.Tokens.Jwt;

namespace LoyaltyCardsWebApi.API.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private const string UserIdKey = "UserId";
    private const string UserEmailKey = "UserEmail";
    private const string IsAuthenticatedKey = "IsAuthenticated";
    private const string ExpClaimType = "exp";

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICurrentUserService currentUserService)
    {
        var isAuthenticated = currentUserService.IsAuthenticated;
        
        using (LogContext.PushProperty(IsAuthenticatedKey, isAuthenticated))
        {
            if (!isAuthenticated)
            {
                await _next(httpContext);
                return;
            }
        
            var userId = currentUserService.UserId;
            var userEmail = currentUserService.UserEmail;
            DateTimeOffset? tokenExpiryTime = null;
            var tokenExpClaim = 
                httpContext.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
                ?? httpContext.User.FindFirst(ExpClaimType)?.Value;
            if (long.TryParse(tokenExpClaim, out var exp))
            {
                tokenExpiryTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                httpContext.Items[ContextKeys.TokenExpTime] = tokenExpiryTime.ToString();
            }

            using (LogContext.PushProperty(UserIdKey, userId))
            using (LogContext.PushProperty(UserEmailKey, userEmail))
            using (LogContext.PushProperty(IsAuthenticatedKey, isAuthenticated))
            using (LogContext.PushProperty(ContextKeys.TokenExpTime, tokenExpiryTime?.ToString("O")))
            {
                await _next(httpContext);
            }
        }
    }
}
