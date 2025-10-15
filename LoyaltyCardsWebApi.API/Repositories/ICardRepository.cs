using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;

public interface ICardRepository
{
    Task<Card?> CreateCardAsync(Card newCard);
    Task<Card?> Delete(int id, int userId);
    Task<bool> ExistsCardByBarcodeAsync(string barcode, int userId);
    Task<Card?> GetCardByIdAsync(int id, int userId);
    Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId);
    Task<Card?> UpdateCardAsync(Card updateCard, int userId);
}
