using System.Text.Json;
using LoyaltyCardsWebApi.API.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.ExceptionHandling;

[TestFixture]
public class GlobalExceptionHandlerTest
{
    private Mock<ILogger<GlobalExceptionHandler>> _logger;
    private GlobalExceptionHandler _handler;
    private HttpContext _context;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_logger.Object);
        _context = new DefaultHttpContext();
    }

    public static IEnumerable<TestCaseData> CreateTestCases()
    {
        yield return new TestCaseData(
            new DbUpdateConcurrencyException(),
            StatusCodes.Status409Conflict,
            "Resource was modified by another user"
            ).SetName("TryHandleAsync_Exception_ResponseProblemDetails_DbUpdateConcurrencyException");
        yield return new TestCaseData(
            new DbUpdateException(),
            StatusCodes.Status409Conflict,
            "Database update error"
            ).SetName("TryHandleAsync_Exception_ResponseProblemDetails_DbUpdateException");
        yield return new TestCaseData(
            new TimeoutException(),
            StatusCodes.Status504GatewayTimeout,
            "Timeout"
            ).SetName("TryHandleAsync_Exception_ResponseProblemDetails_TimeoutException");
        yield return new TestCaseData(
            new OperationCanceledException(),
            StatusCodes.Status499ClientClosedRequest,
            "Request canceled"
            ).SetName("TryHandleAsync_Exception_ResponseProblemDetails_OperationCanceledException");
        yield return new TestCaseData(
            new HttpRequestException(),
            StatusCodes.Status503ServiceUnavailable,
            "Upstream service error"
            ).SetName("TryHandleAsync_Exception_ResponseProblemDetails_HttpRequestException");
    }
    [Test, TestCaseSource(nameof(CreateTestCases))]
    public async Task TryHandleAsync_Exception_ResponseProblemDetails
        (
            Exception exception,
            int statusCode,
            string details
        )
    {
        _context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(_context, exception, default);

        Assert.That(handled, Is.True);
        Assert.That(_context.Response.StatusCode, Is.EqualTo(statusCode));
        Assert.That(_context.Response.ContentType, Is.EqualTo("application/problem+json; charset=utf-8"));
        _context.Response.Body.Position = 0;
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(_context.Response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(problemDetails, Is.Not.Null);
        Assert.That(problemDetails.Title, Is.EqualTo("Exception"));
        Assert.That(problemDetails.Detail, Is.EqualTo(details));
    }
}