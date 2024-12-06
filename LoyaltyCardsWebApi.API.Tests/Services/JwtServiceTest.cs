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
    private string _userId = "123"; 
    private string _userEmail = "user123@test.com";

    [SetUp]
    public void SetUp()
    {
        _configuration = new Mock<IConfiguration>();
        _jwtService = new JwtService(_configuration.Object);
        _validationParameters = CreateValidationParameters();
    }

    private TokenValidationParameters CreateValidationParameters()
    {
        return new TokenValidationParameters
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

    private void SetupJwtSettings(string secretKey, string testIssuer, string testAudience, string expirationMinutes)
    {
        var jwtSettingSection = new Mock<IConfigurationSection>();
        jwtSettingSection.Setup(x => x["JWT_Secret"]).Returns(secretKey);
        jwtSettingSection.Setup(x => x["JWT_Issuer"]).Returns(testIssuer);
        jwtSettingSection.Setup(x => x["JWT_Audience"]).Returns(testAudience);
        jwtSettingSection.Setup(x => x["JWT_ExpirationMinutes"]).Returns(expirationMinutes);

        _configuration?.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingSection.Object);
    }

    [Test]
    public void GenerateToken_ValidInput_ReturnsNotNullAndNotEmptyToken()
    {
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");
        
        var token = _jwtService?.GenerateToken(_userId, _userEmail);
        
        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);
    }

    [Test]
    public void GenerateToken_ValidInput_ValidatesTokenCorrectly()
    {
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");
        
        SecurityToken validatedToken;
        bool isTokenValid = false;
        var tokenHandler = new JwtSecurityTokenHandler();

        var token = _jwtService?.GenerateToken(_userId, _userEmail);
        
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
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");

        var token = _jwtService?.GenerateToken(_userId, _userEmail);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        Assert.That(jwtToken?.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(_userId));
        Assert.That(jwtToken?.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value, Is.EqualTo(_userEmail));
    }

    [Test]
    public void GenerateToken_ValidInput_ContainsCorrectExpiration()
    {
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");
        
        var expirationMinutes = 2;

        var token = _jwtService?.GenerateToken(_userId, _userEmail);
        var expectedExpirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var timeTolerance = TimeSpan.FromSeconds(5);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        Assert.That(expectedExpirationTime, Is.InRange(jwtToken?.ValidTo-timeTolerance, jwtToken?.ValidTo+timeTolerance));
    }

    [Test]
    public void GenerateToken_InvalidSecretKey_ThrowsInvalidOperationException()
    {
        SetupJwtSettings("InvalidSecretKey", "TestIssuer", "TestAudience", "2");      
        
        var exeptionMessage = "Key for JWT authentication is not configured, is empty or not long enough.";

        InvalidOperationException? testEx = Assert.Throws<InvalidOperationException>(() => _jwtService?.GenerateToken(_userId, _userEmail));
        Assert.That(testEx?.Message, Is.EqualTo(exeptionMessage));
    }

    [Test]
    [TestCase(null,"plwp@wopep.nl")]
    [TestCase("","plwp@wopep.nl")]
    [TestCase("-1223","plwp@wopep.nl")]
    public void GenerateToken_NotValidUserId_ThrowsInvalidOperationException(string userId, string userEmail)
    {
        var exeptionMessage = "Can't generate token without valid user ID.";
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");

        InvalidOperationException? testEx = Assert.Throws<InvalidOperationException>(() => _jwtService?.GenerateToken(userId, userEmail));
        Assert.That(testEx?.Message, Is.EqualTo(exeptionMessage));
    }

    [Test]
    [TestCase("1222","")]
    [TestCase("11",null)]
    public void GenerateToken_NotValidUserEmail_ThrowsInvalidOperationException(string userId, string userEmail)
    {
        var exeptionMessage = "Can't generate token without valid email.";
        SetupJwtSettings("SecretKey123456SecretKey123456ab", "TestIssuer", "TestAudience", "2");

        InvalidOperationException? testEx = Assert.Throws<InvalidOperationException>(() => _jwtService?.GenerateToken(userId, userEmail));
        Assert.That(testEx?.Message, Is.EqualTo(exeptionMessage));
    }

}