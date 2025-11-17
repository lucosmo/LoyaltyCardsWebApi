using System.Text;
using System.Text.Json;
using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace LoyaltyCardsWebApi.API.Tests.Authentication;

[TestFixture]
public class JwtBearerEventsCancellationTest()
{
    private CancellationTokenSource _cts;
    private JwtBearerOptions _options;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _cts = new CancellationTokenSource();
        _options = new JwtBearerOptions();
        _httpContext = new DefaultHttpContext();
        _httpContext.RequestAborted = _cts.Token;

        _options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                
                if (context.Response.HasStarted)
                {
                    return;
                }
                                
                var statusCode = StatusCodes.Status401Unauthorized;
                var details = !string.IsNullOrEmpty(context.Error)
                    ? context.Error : !string.IsNullOrEmpty(context.ErrorDescription)
                        ? context.ErrorDescription : "Authentication failed.";
                var title = "Unauthorized.";

                var problemDetails = ProblemDetailsHelper.CreateProblemDetails(
                        context.HttpContext,
                        title,
                        statusCode,
                        details
                    );
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                var json = JsonSerializer.Serialize(problemDetails);
                await context.Response.WriteAsync(json, Encoding.UTF8, context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                var statusCode = StatusCodes.Status403Forbidden;
                var user = context.HttpContext.User;
                var path = context.HttpContext.Request.Path;
                var method = context.HttpContext.Request.Method;
                var details = user.Identity?.IsAuthenticated == true
                    ? $"Access to {method} {path} is forbidden."
                    : $"Access forbidden.";
                var title = "Access forbidden.";

                var problemDetails = ProblemDetailsHelper.CreateProblemDetails(
                        context.HttpContext,
                        title,
                        statusCode,
                        details
                    );
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                var json = JsonSerializer.Serialize(problemDetails);
                await context.Response.WriteAsync(json, Encoding.UTF8, context.HttpContext.RequestAborted);
            }
        };
    }

    [Test]
    public void OnChallenge_RequestAbortedTokenCancelled_PropagatesOperationCancelledException()
    {
        //Arrange
        var authenticationScheme = new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler));
        var challengeContext = new JwtBearerChallengeContext(_httpContext, authenticationScheme, _options, new AuthenticationProperties());

        //Act
        _cts.Cancel();
        Exception ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await _options.Events.OnChallenge(challengeContext);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void OnForbidden_RequestAbortedTokenCancelled_PropagatesOperationCancelledException()
    {
        //Arrange
        var authenticationScheme = new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler));
        var forbiddenContext = new ForbiddenContext(_httpContext, authenticationScheme, _options);

        //Act
        _cts.Cancel();
        Exception ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await _options.Events.OnForbidden(forbiddenContext);
        });

        //Assert
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
    }
}