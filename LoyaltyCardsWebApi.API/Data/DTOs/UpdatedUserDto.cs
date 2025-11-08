using System.ComponentModel.DataAnnotations;
using LoyaltyCardsWebApi.API.Attributes;

namespace LoyaltyCardsWebApi.API.Data.DTOs;
public class UpdatedUserDto
{
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public required string Email { get; set; }
    [ValidatePasswordRules]
    public required string Password { get; set; }
}