using LoyaltyCardsWebApi.API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        _httpContext = new DefaultHttpContext();
        _httpContext.RequestAborted = _cts.Token;

        var services = new ServiceCollection();
        services.AddJwtAuthentication(
            jwtIssuer: "test-issuer",
            jwtAudience: "test-audience",
            secretKey: "secret-key-at-least-32-chars-long"
        );

        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        _options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
        _httpContext.RequestServices = serviceProvider;
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