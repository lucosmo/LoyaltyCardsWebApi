using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Attributes;

public class ValidatePasswordRules : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        List<string> errors = [];
        int minPasswordLength = 12;

        if (value is not string password || string.IsNullOrWhiteSpace(password))
        {
            return new ValidationResult($"{validationContext.DisplayName} is required");
        }

        if (password.Length < minPasswordLength)
        {
            errors.Add($"{validationContext.DisplayName} must contain at least {minPasswordLength} characters.");
        }
        if (!password.Any(char.IsLower))
        {
            errors.Add($"{validationContext.DisplayName} must contain at least 1 lowercase letter.");
        }
        if (!password.Any(char.IsUpper))
        {
            errors.Add($"{validationContext.DisplayName} must contain at least 1 uppercase letter.");
        }
        if (!password.Any(char.IsDigit))
        {
            errors.Add($"{validationContext.DisplayName} must contain at least 1 digit.");
        }
        if (!password.Any(c => "[!$#@&*%()_+=]".Contains(c)))
        {
            errors.Add($"{validationContext.DisplayName} must contain at least 1 special character: !,$,#,@,&,*,%,(,),_,+,= .");
        }
        if (password.Any(char.IsWhiteSpace))
        {
            errors.Add($"{validationContext.DisplayName} cannot contain whitespace.");
        }
        if (errors.Count > 0)
        {
            return new ValidationResult(string.Join(" ", errors)); 
        }
        return ValidationResult.Success;
    }
}