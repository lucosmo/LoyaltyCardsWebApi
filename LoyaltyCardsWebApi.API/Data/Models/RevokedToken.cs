namespace LoyalityCardsWebApi.API.Models;

public class RevokedToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiryTime { get; set; }
    public int UserId {get; set; }
    public User? User { get; set; }
}