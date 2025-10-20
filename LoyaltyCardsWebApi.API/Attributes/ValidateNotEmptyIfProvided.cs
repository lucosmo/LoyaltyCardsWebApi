using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Attributes;

public class ValidateNotEmptyIfProvided : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null && value is not string)
        {
            return new ValidationResult($"{validationContext.DisplayName} has an invalid type.");
        }
        if (value is not null && string.IsNullOrWhiteSpace((string)value))
        {
            return new ValidationResult($"{validationContext.DisplayName} can't be empty or whitespace.");
        }
        return ValidationResult.Success;
    }
}