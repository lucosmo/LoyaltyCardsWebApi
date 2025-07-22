namespace LoyaltyCardsWebApi.API.Data.DTOs
{
    public class UpdatedCardDto
    {
        public required string Name { get; set; }
        public string Image { get; set; } = "https://..default-image.png"; // Default image URL
        public required string Barcode { get; set; }
    }
}
