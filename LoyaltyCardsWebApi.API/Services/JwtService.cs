using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCardsWebApi.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string userEmail)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["JWT_Secret"] ?? string.Empty;
        var expirationMinutes = jwtSettings["JWT_ExpirationMinutes"];
        var issuer = jwtSettings["JWT_Issuer"];
        var audience = jwtSettings["JWT_Audience"];
        int minSecretKeyLength = 32;

        VerifyTokenParameters(userId, userEmail, secretKey, expirationMinutes, minSecretKeyLength);
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        double.TryParse(expirationMinutes, out var expirationTime);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var jwtToken = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            audience: audience,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddMinutes(expirationTime)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private void VerifyTokenParameters(string? userId, string? userEmail, string? secretKey, string? expTime, int minSecretKeyLength)
    {
        if(IsInvalidSecretKey(secretKey, minSecretKeyLength))
        {
            throw new InvalidOperationException("Key for JWT authentication is not configured, is empty or not long enough");
        }
        if (IsInvalidExpirationTime(expTime, out _))
        {
            throw new InvalidOperationException("Wrong expiration time set for token");
        }
        if(IsInvalidUserId(userId))
        {
            throw new InvalidOperationException("Can't generate token without valid user ID");
        }
        if(IsInvalidEmail(userEmail))
        {
            throw new InvalidOperationException("Can't generate token without valid email");
        }
    }
    
    private bool IsInvalidSecretKey(string? secretKey, int minSecretKeyLength)
    {
        return string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < minSecretKeyLength;
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
}