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

    public async Task<Card?> CreateCardAsync(Card newCard)
    {
        var createdCard = await _appDbContext.Cards.AddAsync(newCard);
        await _appDbContext.SaveChangesAsync();
        return createdCard.Entity;
    }

    public async Task<Card?> Delete(int id)
    {
        var cardToDelete = await _appDbContext.Cards.FindAsync(id);
        if (cardToDelete != null)
        {
            _appDbContext.Cards.Remove(cardToDelete);
            await _appDbContext.SaveChangesAsync();
            return cardToDelete;
        }
        else
        {
            return null;
        }
    }

    public async Task<Card?> GetCardByIdAsync(int id)
    {
        var card = await _appDbContext.Cards
            .FirstOrDefaultAsync(c => c.Id == id);
        return card;
    }

    public Task<IEnumerable<Card>> GetCards()
    {
        throw new NotImplementedException();
    }

    public async Task<Card?> UpdateCardAsync(Card updateCard)
    {
        var existingCard = await _appDbContext.Cards.FindAsync(updateCard.Id);
        if (existingCard != null)
        {
            existingCard.Name = updateCard.Name;
            existingCard.Image = updateCard.Image;
            existingCard.Barcode = updateCard.Barcode;
            _appDbContext.Cards.Update(existingCard);
            await _appDbContext.SaveChangesAsync();
            return existingCard;
        }
        else
        {
            return null;
        }
    }

    public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(int? id)
    {
        var cards = await _appDbContext.Cards.Where(c => c.UserId == id).ToListAsync();
        return cards;
    }

    public async Task<bool> ExistsCardByBarcodeAsync(string barcode, int? userId)
    {
        return await _appDbContext.Cards.AnyAsync(c => c.Barcode == barcode && c.UserId == userId);
    }
}