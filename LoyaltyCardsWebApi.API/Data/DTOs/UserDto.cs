namespace LoyalityCardsWebApi.API.Data.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
}