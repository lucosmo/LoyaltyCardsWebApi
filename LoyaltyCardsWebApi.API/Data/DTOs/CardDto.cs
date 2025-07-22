namespace LoyaltyCardsWebApi.API.Data.DTOs
{
    public class CardDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required int UserId { get; set; }
    }
}
