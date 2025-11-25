using System.Security.Claims;
using LoyaltyCardsWebApi.API.Data;
using Serilog.Context;

namespace LoyaltyCardsWebApi.API.Middleware;

public class RequestLoggingContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var requestContext = httpContext.RequestServices.GetRequiredService<IRequestContext>();
        using (LogContext.PushProperty("CorrelationId", requestContext.CorrelationId ?? string.Empty))
        using (LogContext.PushProperty("TraceId", requestContext.TraceIdentifier ?? string.Empty))
        using (LogContext.PushProperty("UserId", requestContext.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value ?? "anonymous"))
        using (LogContext.PushProperty("Path", httpContext.Request.Path.Value ?? string.Empty))
        using (LogContext.PushProperty("Method", httpContext.Request.Method))
        using (LogContext.PushProperty("UserAgent", httpContext.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("Ip", requestContext.IpAddress ?? string.Empty))
        {
            await _next(httpContext);
        }
    }
}