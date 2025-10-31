using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Common;
public static class ProblemDetailsHelper
{
    public static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        string title,
        int statusCode,
        string? details
        )
    {
        string instance;
        string requestId;
        string traceId;

        (instance, requestId, traceId) = PrepareMissingValues(httpContext);
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = details,
            Instance = instance
        };
        problemDetails.Extensions["requestId"] = requestId;
        problemDetails.Extensions["traceId"] = traceId;
        return problemDetails;
    }

    private static (string, string, string) PrepareMissingValues(HttpContext httpContext)
    {
        var instance = $"{httpContext.Request.Method} {httpContext.Request.Path}" ?? string.Empty;
        var requestId = httpContext.TraceIdentifier ?? string.Empty;
        Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        var traceId = activity?.Id ?? string.Empty;

        return (instance, requestId, traceId);
    }
}