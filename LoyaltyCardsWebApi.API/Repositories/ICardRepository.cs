using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;
public interface ICardRepository
{
    Task<IEnumerable<Card>> GetCards();
    Task<Card> GetCardByIdAsync(int id);
    Task<IEnumerable<Card>> GetCardsByUserIdAsync(int id);
    Task<Card> CreateCardAsync(Card newCard);
    Task<Card> UpdateCardAsync(int id, UpdatedCardDto card);
    Task<Card> Delete(int id);
}
