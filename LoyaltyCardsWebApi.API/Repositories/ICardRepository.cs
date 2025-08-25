using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;

public interface ICardRepository
{
    Task<Card?> CreateCardAsync(Card newCard);
    Task<Card?> Delete(int id);
    Task<bool> ExistsCardByBarcodeAsync(string barcode, int? userId);
    Task<IEnumerable<Card>> GetCards();
    Task<Card?> GetCardByIdAsync(int id);
    Task<IEnumerable<Card>> GetCardsByUserIdAsync(int? id);
    Task<Card?> UpdateCardAsync(Card updateCard);
}
