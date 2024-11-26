using System.IdentityModel.Tokens.Jwt;
using System.Text;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Services;

public class JwtServiceTest
{
    private Mock<IConfiguration>? _configuration;
    private JwtService? _jwtService;
    private TokenValidationParameters? _validationParameters;
    [SetUp]
    public void SetUp()
    {
        _configuration = new Mock<IConfiguration>();
        _jwtService = new JwtService(_configuration.Object);
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TestIssuer",
            ValidAudience = "TestAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SecretKey123456SecretKey123456ab"))
        };
    }

    [Test]
    public void GenerateToken_ValidInput_ReturnsNotEmptyToken()
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
        
        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);
    }

    [Test]
    public void GenerateToken_ValidInput_ValidatesTokenCorrectly()
    {
        var jwtSettingSection = new Mock<IConfigurationSection>();
        jwtSettingSection.Setup(x => x["JWT_Secret"]).Returns("SecretKey123456SecretKey123456ab");
        jwtSettingSection.Setup(x => x["JWT_Issuer"]).Returns("TestIssuer");
        jwtSettingSection.Setup(x => x["JWT_Audience"]).Returns("TestAudience");
        jwtSettingSection.Setup(x => x["JWT_ExpirationMinutes"]).Returns("2");

        _configuration?.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingSection.Object);

        var userId = "123";
        var userEmail = "user123@test.com";

        SecurityToken validatedToken;
        bool isTokenValid = false;
        var tokenHandler = new JwtSecurityTokenHandler();

        var token = _jwtService?.GenerateToken(userId, userEmail);
        
        try
        {
            tokenHandler.ValidateToken(token, _validationParameters, out validatedToken);
            isTokenValid = true;
        } 
        catch (SecurityTokenValidationException)
        {
            isTokenValid = false;
        }

        Assert.IsTrue(isTokenValid);
    }

    [Test]
    public void GenerateToken_ValidInput_ContainsCorrectClaims()
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
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        Assert.That(jwtToken?.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(userId));
        Assert.That(jwtToken?.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value, Is.EqualTo(userEmail));
    }

    [Test]
    public void GenerateToken_ValidInput_ContainsCorrectExpiration()
    {
        var jwtSettingSection = new Mock<IConfigurationSection>();
        jwtSettingSection.Setup(x => x["JWT_Secret"]).Returns("SecretKey123456SecretKey123456ab");
        jwtSettingSection.Setup(x => x["JWT_Issuer"]).Returns("TestIssuer");
        jwtSettingSection.Setup(x => x["JWT_Audience"]).Returns("TestAudience");
        jwtSettingSection.Setup(x => x["JWT_ExpirationMinutes"]).Returns("2");

        _configuration?.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingSection.Object);

        var userId = "123";
        var userEmail = "user123@test.com";
        var expirationMinutes = 2;

        var token = _jwtService?.GenerateToken(userId, userEmail);
        var expectedExpirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var timeTolerance = TimeSpan.FromSeconds(5);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        Assert.That(expectedExpirationTime, Is.InRange(jwtToken?.ValidTo-timeTolerance, jwtToken?.ValidTo+timeTolerance));
    }

    [Test]
    public void GenerateToken_InvalidSecretKey_ThrowsInvalidOperationException()
    {
        var jwtSettingSection = new Mock<IConfigurationSection>();
        jwtSettingSection.Setup(x => x["JWT_Secret"]).Returns("InvalidSecretKey");
        jwtSettingSection.Setup(x => x["JWT_Issuer"]).Returns("TestIssuer");
        jwtSettingSection.Setup(x => x["JWT_Audience"]).Returns("TestAudience");
        jwtSettingSection.Setup(x => x["JWT_ExpirationMinutes"]).Returns("2");

        _configuration?.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingSection.Object);

        var userId = "123";
        var userEmail = "user123@test.com";
        var exeptionMessage = "Key for JWT authentication is not configured, is empty or not long enough";

        InvalidOperationException? testEx = Assert.Throws<InvalidOperationException>(() => _jwtService?.GenerateToken(userId, userEmail));
        Assert.That(testEx?.Message, Is.EqualTo(exeptionMessage));
    }

}