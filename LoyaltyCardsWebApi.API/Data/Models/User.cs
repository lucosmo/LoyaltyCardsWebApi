using LoyaltyCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Models;
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
}