using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Attributes;

public class ValidateNotEmptyIfProvided : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }
        if (value is not string)
        {
            throw new ValidationException($"{validationContext.DisplayName} must be a string.");
        }
        if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
        {
            return new ValidationResult($"{validationContext.DisplayName} can't be empty or whitespace.");
        }
        return ValidationResult.Success;
    }
}