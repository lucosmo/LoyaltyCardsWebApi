using System.ComponentModel.DataAnnotations;

namespace LoyalityCardsWebApi.API.Data.DTOs;
public class CreateUserDto
    {
        public required string UserName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public required string Password { get; set; }
    }