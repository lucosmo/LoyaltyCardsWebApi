namespace LoyaltyCardsWebApi.API.Data.DTOs
{
    public class UpdateCardDto
    {
        public required string Name { get; set; }
        public required string Image { get; set; }
        public required string Barcode { get; set; }
    }
}
