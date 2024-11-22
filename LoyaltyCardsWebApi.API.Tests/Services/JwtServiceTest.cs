using LoyaltyCardsWebApi.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

public class JwtServiceTest
{
    private Mock<IConfiguration>? _configuration;
    private JwtService? _jwtService;
    [SetUp]
    public void SetUp()
    {
        _configuration = new Mock<IConfiguration>();
        _jwtService = new JwtService(_configuration.Object);
    }

    [Test]
    public void GenerateToken_ValidInput_ReturnsValidJwtToken()
    {
        var jwtSettingSection = new Mock<IConfigurationSection>();
        jwtSettingSection.Setup(x => x["JWT_Secret"]).Returns("SecretKey123456SecretKey123456ab");
        jwtSettingSection.Setup(x => x["JWT_Issuer"]).Returns("TestIssuer");
        jwtSettingSection.Setup(x => x["JWT_Audience"]).Returns("TestAudience");
        jwtSettingSection.Setup(x => x["JWT_ExpirationMinutes"]).Returns("2");

        _configuration?.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingSection.Object);

        var userId = "123";
        var userEmail = "user123@test.com";

        var token = _jwtService?.GenerateToken(userId, userEmail);

        Console.WriteLine(token);

        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);

    }

}