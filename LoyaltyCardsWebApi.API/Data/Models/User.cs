using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyalityCardsWebApi.API.Models;
public class User
{
    public int Id { get; set;}
    public required string UserName { get; set;}
    public required string Password { get; set;}
    public required string Email { get; set;}
    public string? FirstName { get; set;}
    public string? LastName { get; set;}
    public DateTime AccountCreatedDate { get; set;}
    public List<Card>? Cards {get; set;}
    public string? Settings { get; set;}

    public User()
    {
        
    }

    public User(int id, string userName, string password, string email, string firstName, string lastName, DateTime accountCreatedDate, List<Card> cards, string settings)
    {
        Id = id;
        UserName = userName;
        Password = password;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        AccountCreatedDate = accountCreatedDate;
        Cards = cards;
        Settings = settings;
    }
}