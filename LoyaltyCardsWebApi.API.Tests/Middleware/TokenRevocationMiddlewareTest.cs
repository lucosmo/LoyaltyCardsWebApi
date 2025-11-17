using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Middleware;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Middleware;

[TestFixture]
public class TokenRevocationMiddlewareTest
{
    private Mock<RequestDelegate> _requestDelegate;
    private DefaultHttpContext _httpContext;
    private Mock<IAuthService> _authService;
    private Mock<IProblemDetailsService> _problemDetailsService;

    [SetUp]
    public void SetUp()
    {
        _requestDelegate = new Mock<RequestDelegate>();
        _httpContext = new DefaultHttpContext();
        _authService = new Mock<IAuthService>();
        _problemDetailsService = new Mock<IProblemDetailsService>();
    }

    [Test]
    public void InvokeAsync_RequestAbortedTokenCancelled_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var middleware = new TokenRevocationMiddleware(_requestDelegate.Object);
        _httpContext.RequestAborted = cts.Token;
        cts.Cancel();
        _authService
            .Setup(a => a.GetTokenAuthHeader())
            .Returns(Result<string>.Ok("Ok"));
        _authService
            .Setup(a => a.IsTokenRevokedAsync(It.IsAny<string>(),It.Is<CancellationToken>(ct => ct == cts.Token)))
            .Returns(Task.FromCanceled<bool>(cts.Token));
        Exception ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await middleware.InvokeAsync(_httpContext, _authService.Object, _problemDetailsService.Object);
        });

        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _authService.Verify(a => a.GetTokenAuthHeader(), Times.Once);
        _authService.Verify(a => a.IsTokenRevokedAsync(It.IsAny<string>(), cts.Token), Times.Once);
        _requestDelegate.Verify(rd => rd.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

     [Test]
    public async Task InvokeAsync_RequestAbortedNotCancelled_CallsNextAndDoesNotThrow()
    {
        using var cts = new CancellationTokenSource();
        var middleware = new TokenRevocationMiddleware(_requestDelegate.Object);
        _httpContext.RequestAborted = cts.Token;

        _authService
            .Setup(a => a.GetTokenAuthHeader())
            .Returns(Result<string>.Ok("Ok"));
        _authService
            .Setup(a => a.IsTokenRevokedAsync("Ok", cts.Token))
            .ReturnsAsync(false);

        await middleware.InvokeAsync(_httpContext, _authService.Object, _problemDetailsService.Object);

        _authService.Verify(a => a.GetTokenAuthHeader(), Times.Once);
        _authService.Verify(a => a.IsTokenRevokedAsync("Ok", cts.Token), Times.Once);
        _requestDelegate.Verify(rd => rd.Invoke(It.IsAny<HttpContext>()), Times.Once);
    }

     [Test]
    public void InvokeAsync_RequestAbortedTokenCancelledDuringRevocationCheck_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var middleware = new TokenRevocationMiddleware(_requestDelegate.Object);
        _httpContext.RequestAborted = cts.Token;
        _authService
            .Setup(a => a.GetTokenAuthHeader())
            .Returns(Result<string>.Ok("Ok"));
        _authService
            .Setup(a => a.IsTokenRevokedAsync("Ok", cts.Token))
            .Returns(async () =>
            {
                cts.Cancel();
                cts.Token.ThrowIfCancellationRequested();
                return false;
            });
        Exception ex = Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await middleware.InvokeAsync(_httpContext, _authService.Object, _problemDetailsService.Object);
        });

        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        _authService.Verify(a => a.GetTokenAuthHeader(), Times.Once);
        _authService.Verify(a => a.IsTokenRevokedAsync("Ok", cts.Token), Times.Once);
        _requestDelegate.Verify(rd => rd.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }
}