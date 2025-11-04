using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Controllers.Results;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Controllers.Results;

[TestFixture]
public class ApiResultTest
{
    ActionContext actionContext;
    [SetUp]
    public void Setup()
    {
        actionContext = new ActionContext()
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddControllers();
        actionContext.HttpContext.RequestServices = services.BuildServiceProvider();
    }

    [Test]
    public async Task ExecuteResultAsync_ResultNoContent_ResponseSuccess()
    {
        var result = new ApiResult<object?>(Result<object?>.NoContent());

        await result.ExecuteResultAsync(actionContext);

        Assert.That(actionContext.HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        Assert.That(actionContext.HttpContext.Response.ContentLength, Is.Null);
    }

    [Test]
    public async Task ExecuteResultAsync_ResultUnauthorized_ResponseIsProblemDetails()
    {
        var result = new ApiResult<object?>(Result<object?>.Unauthorized("Unauthorized"));

        await result.ExecuteResultAsync(actionContext);

        Assert.That(actionContext.HttpContext.Response.ContentType, Is.EqualTo("application/problem+json; charset=utf-8"));
        Assert.That(actionContext.HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

    }

    [Test]
    public async Task ExecuteResultAsync_ResultCreated_ResponseSuccess()
    {
        var result = new ApiResult<object?>(Result<object?>.Created("Created", "/api/users/1"));

        await result.ExecuteResultAsync(actionContext);

        Assert.That(actionContext.HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(actionContext.HttpContext.Response.Headers.Location, Is.EqualTo("/api/users/1"));
    }
}