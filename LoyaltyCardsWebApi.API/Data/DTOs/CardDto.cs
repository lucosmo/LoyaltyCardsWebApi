namespace LoyalityCardsWebApi.API.Data.DTOs;

public class CardDto
{
    public int Id { get; set;}
    public required string Name { get; set;}
    public string? Image { get; set;}
    public required string Barcode { get; set;}
}