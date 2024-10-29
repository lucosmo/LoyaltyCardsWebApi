using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Services;
public interface ICardService
{
    Task<Card> CreateCardAsync(CreateCardDto newCard);
    Task<List<Card>?> GetAllCardsAsync();
    Task<Card?> GetCardByIdAsync(int id);
    Task<Card?> DeleteAsync(int id);
    Task<bool> UpdateCardAsync(int id, UpdatedCardDto updatedCard);
}