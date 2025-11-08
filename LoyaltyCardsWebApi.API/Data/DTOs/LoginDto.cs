using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Data.DTOs;

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; init; }
    public required string Password { get; init; }
}