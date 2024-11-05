using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface ICardRepository
{
    Task<IEnumerable<Card>?> GetCardsAsync();
    Task<Card?> GetCardByIdAsync(int id);
    Task<Card> CreateAsync(Card newCard);
    Task<bool> UpdateAsync(Card card);
    Task<Card?> DeleteAsync(int id);
}
