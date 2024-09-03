namespace LoyalityCardsWebApi.API.Models;

public class Card 
{
    public int Id { get; set;}
    public required string Name { get; set;}
    public required string Image { get; set;}
    public required string Barcode { get; set;}
    public DateTime AddedAt { get; set;}
    public int UserId { get; set; }
    public required User User { get; set; }

    public Card()
    {
        
    }

    public Card(int id, string name, string image, string barcode, DateTime addedAt, int userId, User user)
    {
        Id = id;
        Name = name;
        Image = image;
        Barcode = barcode;
        AddedAt = addedAt;
        UserId = userId;
        User = user;
    }
}

