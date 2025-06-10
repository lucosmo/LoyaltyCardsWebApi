using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Data.DTOs;

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; init; }
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public required string Password { get; init; }
}