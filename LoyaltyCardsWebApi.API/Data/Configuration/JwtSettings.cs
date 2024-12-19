namespace LoyalityCardsWebApi.API.Data.Configuration;

public record JwtSettings(string SecretKey, string ExpirationMinutes, string Issuer, string Audience);