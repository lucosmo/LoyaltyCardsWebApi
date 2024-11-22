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
        if(string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Key for JWT authentication is not configured or is empty");
        }
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expirationMinutes = 0.0;
        if(double.TryParse(jwtSettings["JWT_ExpirationMinutes"], out double parsedExpirationMinutes))
        {
            expirationMinutes = parsedExpirationMinutes;
        }
        else
        {
            throw new InvalidOperationException("Wrong expiration time set for token");
        }

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
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}