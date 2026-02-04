using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serilog.Context;
using System.Net.Mime;
using System.Security.Claims;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace LoyaltyCardsWebApi.API.Middleware;

public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string DeviceIdHeaderName = "X-Device-Id";
    private const string GuestIdHeaderName = "X-Guest-ID";
    private const string SessionIdHeaderName = "X-Session-ID";

    private const string SessionIdCookieName = "SID";
    private const string DeviceIdCookieName = "DID";

    private const int DefaultCookieExpiryYears = 2;
    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IDateTimeProvider dateTimeProvider)
    { 
        var correlationId = GetHeaderValue(httpContext, CorrelationIdHeaderName);
        httpContext.Items[ContextKeys.CorrelationIdKey] = correlationId;
        httpContext.Response.Headers[CorrelationIdHeaderName] = correlationId;
        var guestId = GetHeaderValue(httpContext, GuestIdHeaderName);
        httpContext.Items[ContextKeys.GuestIdKey] = guestId;
        httpContext.Response.Headers[GuestIdHeaderName] = guestId;
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        httpContext.Items[ContextKeys.UserAgentKey] = userAgent;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        httpContext.Items[ContextKeys.ClientIpKey] = clientIp;
        var deviceId = GetDeviceId(httpContext, dateTimeProvider);
        httpContext.Items[ContextKeys.DeviceIdKey] = deviceId;
        var sessionId = GetSessionId(httpContext);
        httpContext.Items[ContextKeys.SessionIdKey] = sessionId;
        var queryParams = httpContext.Request.QueryString.Value ?? string.Empty;
        httpContext.Items[ContextKeys.QueryParamsKey] = queryParams;

        using (LogContext.PushProperty(ContextKeys.ClientIpKey, clientIp))
        using (LogContext.PushProperty(ContextKeys.CorrelationIdKey, correlationId))
        using (LogContext.PushProperty(ContextKeys.DeviceIdKey, deviceId))
        using (LogContext.PushProperty(ContextKeys.GuestIdKey, guestId))
        using (LogContext.PushProperty(ContextKeys.SessionIdKey, sessionId))
        using (LogContext.PushProperty(ContextKeys.UserAgentKey, userAgent))
        using (LogContext.PushProperty(ContextKeys.QueryParamsKey, queryParams))
        {
            await _next(httpContext);
        }
    }

    private string GetSessionId(HttpContext httpContext)
    {
        if (TryGetFromCookieOrHeader(httpContext, SessionIdCookieName, SessionIdHeaderName, out var sid))
        {
            return sid;
        }

        var newId = Guid.NewGuid().ToString();
        SetCookie(httpContext, SessionIdCookieName, newId, false);
        httpContext.Response.Headers[SessionIdHeaderName] = newId;
        return newId;
    }

    private string GetDeviceId(HttpContext httpContext, IDateTimeProvider dateTimeProvider)
    {
        if (TryGetFromCookieOrHeader(httpContext, DeviceIdCookieName, DeviceIdHeaderName, out var did))
        {
            return did;
        }

        var newId = Guid.NewGuid().ToString();
        SetCookie(httpContext, DeviceIdCookieName, newId, true, dateTimeProvider);
        httpContext.Response.Headers[DeviceIdHeaderName] = newId;
        return newId;
    }

    private static string GetHeaderValue(HttpContext context, string name)
    {
        if (context.Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.ToString();
        }

        var id = Guid.NewGuid().ToString();
        return id;
    }
    private void SetCookie(HttpContext httpContext, string cookieName, string id, bool persistent, IDateTimeProvider? dateTimeProvider = null)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };
        if (persistent && dateTimeProvider is not null)
        {
            options.Expires = dateTimeProvider.UtcNow.AddYears(DefaultCookieExpiryYears);
            httpContext.Response.Cookies.Append(cookieName, id, options);
        }
        else
        {
            httpContext.Response.Cookies.Append(cookieName, id, options);
        }
    }

    private bool TryGetFromCookieOrHeader(HttpContext httpContext, string cookieName, string headerName, out string id)
    {
        string? fromCookie = httpContext.Request.Cookies[cookieName];
        if (string.IsNullOrEmpty(fromCookie))
        {
            string? fromHeader = httpContext.Request.Headers[headerName].FirstOrDefault();
            if (string.IsNullOrEmpty(fromHeader))
            {
                id = string.Empty;
                return false;
            }
            id = fromHeader;
            return true;
        }
        id = fromCookie;
        return true;
    }
}