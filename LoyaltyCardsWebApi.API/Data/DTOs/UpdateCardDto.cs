using System.ComponentModel.DataAnnotations;
using LoyaltyCardsWebApi.API.Attributes;

namespace LoyaltyCardsWebApi.API.Data.DTOs
{
    public class UpdateCardDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        [ValidateNotEmptyIfProvided]
        public string? Name { get; set; }
        [Url(ErrorMessage = "Image must be a valid URL.")]
        [ValidateNotEmptyIfProvided]
        public string? Image { get; set; }
        [ValidateNotEmptyIfProvided]
        public string? Barcode { get; set; }
    }
}
