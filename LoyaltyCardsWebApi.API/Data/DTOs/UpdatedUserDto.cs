using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Data.DTOs;
public class UpdatedUserDto
{
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; set; }
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public required string Password { get; set; }
}