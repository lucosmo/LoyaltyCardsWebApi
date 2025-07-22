using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyCardsWebApi.API.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _appDbContext;

    public CardRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Card> CreateCardAsync(Card newCard)
    {
        var createdCard = await _appDbContext.Cards.AddAsync(newCard);
        await _appDbContext.SaveChangesAsync();
        return createdCard.Entity;
    }

    public Task<Card> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Card> GetCardByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Card>> GetCards()
    {
        throw new NotImplementedException();
    }

    public Task<Card> UpdateCardAsync(int id, UpdatedCardDto card)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(int id)
    {
        var cards = await _appDbContext.Cards.Where(c => c.UserId == id).ToListAsync();
        return cards;
    }
}