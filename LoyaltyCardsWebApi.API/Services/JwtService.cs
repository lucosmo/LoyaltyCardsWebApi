using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoyaltyCardsWebApi.API.Data.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCardsWebApi.API.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private const int MinSecretKeyLength = 32;

    public JwtService(IConfiguration configuration)
    {
        _jwtSettings = GetJwtSettings(configuration);
    }

    public string GenerateToken(string userId, string userEmail)
    {
        VerifyTokenParameters(userId, userEmail);
        var credentials = CreateSigningCredentials(_jwtSettings.SecretKey);
        var claims = CreateClaims(userId, userEmail);

        var jwtToken = CreateJwtToken(claims, _jwtSettings, credentials);
        
        return SerializeJwtToken(jwtToken);
    }

    private JwtSettings GetJwtSettings(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        return new JwtSettings(
            SecretKey: jwtSettings["JWT_Secret"] ?? string.Empty,
            ExpirationMinutes: jwtSettings["JWT_ExpirationMinutes"] ?? string.Empty,
            Issuer: jwtSettings["JWT_Issuer"] ?? string.Empty,
            Audience: jwtSettings["JWT_Audience"] ?? string.Empty
        );
    }

    private void VerifyTokenParameters(string? userId, string? userEmail)
    {
        if (IsInvalidSecretKey(_jwtSettings.SecretKey))
        {
            throw new ArgumentException("Key for JWT authentication is not configured, is empty or not long enough.");
        }
        if (IsInvalidExpirationTime(_jwtSettings.ExpirationMinutes, out _))
        {
            throw new ArgumentException("Wrong expiration time set for token.");
        }
        if (IsInvalidUserId(userId))
        {
            throw new ArgumentException("Can't generate token without valid user ID.");
        }
        if (IsInvalidEmail(userEmail))
        {
            throw new ArgumentException("Can't generate token without valid email.");
        }
    }
    
    private bool IsInvalidSecretKey(string? secretKey)
    {
        return string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < MinSecretKeyLength;
    }

    private bool IsInvalidUserId(string? userId)
    {
        return int.TryParse(userId, out int convertedInt) == false || convertedInt < 0;
    }

    private bool IsInvalidExpirationTime(string? expTime, out double expirationTime)
    {
        return double.TryParse(expTime, out expirationTime) == false || expirationTime <= 0;
    }

    private bool IsInvalidEmail(string? userEmail)
    {
        return string.IsNullOrWhiteSpace(userEmail);
    }

    private SigningCredentials CreateSigningCredentials(string secretKey)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }

    private IEnumerable<Claim> CreateClaims(string userId, string userEmail)
    {
        return new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail)
        };
    }

    private SecurityToken CreateJwtToken(IEnumerable<Claim> claims, JwtSettings jwtSettings, SigningCredentials credentials)
    {
        double.TryParse(jwtSettings.ExpirationMinutes, out var expirationTime);
        
        return new JwtSecurityToken(
            claims: claims,
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddMinutes(expirationTime)
        );
    }

    private string SerializeJwtToken(SecurityToken jwtToken)
    {
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    

}