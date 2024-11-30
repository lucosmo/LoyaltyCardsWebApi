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
        var secretKey = jwtSettings["JWT_Secret"];
        var expirationMinutes = jwtSettings["JWT_ExpirationMinutes"];
        int minSecretKeyLength = 32;
        
        VerifyParameters(userId, userEmail, secretKey, expirationMinutes, minSecretKeyLength);
        
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
            issuer: jwtSettings["JWT_Issuer"],
            audience: jwtSettings["JWT_Audience"],
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddMinutes(expirationTime)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private void VerifyParameters(string userId, string userEmail, string secretKey, string expTime, int minSecretKeyLength)
    {
        if(string.IsNullOrEmpty(secretKey) || secretKey.Length < minSecretKeyLength)
        {
            throw new InvalidOperationException("Key for JWT authentication is not configured, is empty or not long enough");
        }
        if(string.IsNullOrEmpty(expTime) || IsExpirationTimeDouble(expTime ?? string.Empty,out double expirationTime) == false)
        {
            throw new InvalidOperationException("Wrong expiration time set for token");
        }
        if(string.IsNullOrEmpty(userId) || IsUserIdInteger(userId) == false || string.IsNullOrEmpty(userEmail))
        {
            throw new InvalidOperationException("Can't generate token without valid parameters");
        }
    }
    private bool IsUserIdInteger(string userId)
    {
        return int.TryParse(userId, out int convertedInt) && convertedInt >= 0;
    }

    private bool IsExpirationTimeDouble(string expTime, out double expirationTime)
    {
        return double.TryParse(expTime, out expirationTime) && expirationTime > 0;
    }
}