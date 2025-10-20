using System.ComponentModel.DataAnnotations;

namespace LoyaltyCardsWebApi.API.Data.DTOs
{
    public class CardDto
    {
        public int Id { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public required string Name { get; set; }
        [Url(ErrorMessage = "Image must be a valid URL.")]
        public required string Image { get; set; }
        public required string Barcode { get; set; }
        public DateTime AddedAt { get; set; }
        public int UserId { get; set; }
    }
}
