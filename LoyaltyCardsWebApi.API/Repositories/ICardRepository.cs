using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;

public interface ICardRepository
{
    Task<Card?> CreateCardAsync(Card newCard, CancellationToken cancellationToken = default);
    Task<Card?> Delete(int id, int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsCardByBarcodeAsync(string barcode, int userId, CancellationToken cancellationToken = default);
    Task<Card?> GetCardByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Card?> UpdateCardAsync(Card updateCard, int userId, CancellationToken cancellationToken = default);
}
