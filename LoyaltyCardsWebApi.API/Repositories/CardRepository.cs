using LoyalityCardsWebApi.API.Models;

namespace LoyalityCardsWebApi.API.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _appDbContext;

    public CardRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Card> CreateAsync(Card newCard)
    {
        var createdCard = await _appDbContext.Cards.AddAsync(newCard);
        await _appDbContext.SaveChangesAsync();
        return createdCard.Entity;
    }

    public async Task<Card?> DeleteAsync(int id)
    {
        Card? cardToDelete = await _appDbContext.Cards.FindAsync(id);
        if (cardToDelete != null)
        {
            _appDbContext.Cards.Remove(cardToDelete);
            await _appDbContext.SaveChangesAsync();
        }
        return cardToDelete;
    }

    public Task<Card?> GetCardByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Card>?> GetCardsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Card card)
    {
        throw new NotImplementedException();
    }
}