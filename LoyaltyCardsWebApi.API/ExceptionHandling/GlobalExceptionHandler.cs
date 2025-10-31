using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyCardsWebApi.API.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            var (status, details) = GetStatusCodeAndTitle(exception);
            var title = "Exception";
            var problemDetails = ProblemDetailsHelper.CreateProblemDetails(context, title, status, details);

            context.Response.StatusCode = problemDetails.Status ?? 500;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails, ct);

            return true;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message?.Contains("duplicate key") == true ||
                   ex.InnerException?.Message?.Contains("UNIQUE constraint") == true;
        }

        private static (int, string) GetStatusCodeAndTitle(Exception exception)
        {
            return exception switch
            {
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Resource was modified by another user"),
                DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (StatusCodes.Status409Conflict, "Resource already exists"),
                DbUpdateException => (StatusCodes.Status409Conflict, "Database update error"),
                TimeoutException => (StatusCodes.Status504GatewayTimeout, "Timeout"),
                OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request canceled"),
                HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Upstream service error"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };
        }
    }
}