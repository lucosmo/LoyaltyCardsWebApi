using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCardsWebApi.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private const int MinSecretKeyLength = 32;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string userEmail)
    {
        var jwtSettings = GetJwtSetting();
        VerifyTokenParameters(userId, userEmail, jwtSettings.SecretKey, jwtSettings.ExpirationMinutes);
        var credentials = CreateSigningCredentials(jwtSettings.SecretKey);
        var claims = CreateClaims(userId, userEmail);

        var jwtToken = CreateJwtToken(claims, jwtSettings, credentials);
        
        return SerializeJwtToken(jwtToken);
    }

    private (string? SecretKey, string? ExpirationMinutes, string? Issuer, string? Audience) GetJwtSetting()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return (
            SecretKey: jwtSettings["JWT_Secret"] ?? string.Empty,
            ExpirationMinutes: jwtSettings["JWT_ExpirationMinutes"],
            Issuer: jwtSettings["JWT_Issuer"],
            Audience: jwtSettings["JWT_Audience"]
        );
    }

    private void VerifyTokenParameters(string? userId, string? userEmail, string? secretKey, string? expTime)
    {
        string message = string.Empty;
        if(IsInvalidSecretKey(secretKey))
        {
            message = "Key for JWT authentication is not configured, is empty or not long enough.";
        }
        if (IsInvalidExpirationTime(expTime, out _))
        {
            message = "Wrong expiration time set for token.";
        }
        if(IsInvalidUserId(userId))
        {
            message = "Can't generate token without valid user ID.";
        }
        if(IsInvalidEmail(userEmail))
        {
            message = "Can't generate token without valid email.";
        }
        if (!string.IsNullOrEmpty(message))
        {
            throw new InvalidOperationException(message);
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
        return string.IsNullOrEmpty(userEmail);
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
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
    }

    private SecurityToken CreateJwtToken(IEnumerable<Claim> claims, (string? SecretKey, string? ExpirationMinutes, string? Issuer, string? Audience) jwtSettings, SigningCredentials credentials)
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