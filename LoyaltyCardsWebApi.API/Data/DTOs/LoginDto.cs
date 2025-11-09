using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Data.DTOs;

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; init; }
    [Required(ErrorMessage = "Password is required")]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long")]
    public required string Password { get; init; }
}