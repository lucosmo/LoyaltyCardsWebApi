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

    public async Task<Card?> CreateCardAsync(Card newCard, CancellationToken cancellationToken = default)
    {
        var createdCard = await _appDbContext.Cards.AddAsync(newCard);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return createdCard.Entity;
    }

    public async Task<Card?> Delete(int id, int userId, CancellationToken cancellationToken = default)
    {
        var cardToDelete = await _appDbContext.Cards
            .Where(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (cardToDelete != null)
        {
            _appDbContext.Cards.Remove(cardToDelete);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return cardToDelete;
        }
        else
        {
            return null;
        }
    }

    public async Task<Card?> GetCardByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var card = await _appDbContext.Cards
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
        return card;
    }

    public async Task<Card?> UpdateCardAsync(Card updateCard, int userId, CancellationToken cancellationToken = default)
    {
        var existingCard = await _appDbContext.Cards
            .Where(c => c.Id == updateCard.Id && c.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingCard != null)
        {
            existingCard.Name = updateCard.Name;
            existingCard.Image = updateCard.Image;
            existingCard.Barcode = updateCard.Barcode;
            _appDbContext.Cards.Update(existingCard);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return existingCard;
        }
        else
        {
            return null;
        }
    }

    public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cards = await _appDbContext.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
        return cards;
    }

    public async Task<bool> ExistsCardByBarcodeAsync(string barcode, int userId, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Cards
                    .AnyAsync(c => c.Barcode == barcode && c.UserId == userId, cancellationToken);
    }
}