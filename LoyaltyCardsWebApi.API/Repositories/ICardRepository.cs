using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;
public interface ICardRepository
{
    Task<IEnumerable<Card>> GetCards();
    Task<Card> GetCardById(int id);
    Task<Card> Create(Card newCard);
    Task<Card> Update(Card card);
    Task<Card> Delete(int id);
}
