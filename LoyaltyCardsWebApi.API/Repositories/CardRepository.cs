using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _appDbContext;

    public CardRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public Task<Card> Create(Card newCard)
    {
        throw new NotImplementedException();
    }

    public Task<Card> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Card> GetCardById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Card>> GetCards()
    {
        throw new NotImplementedException();
    }

    public Task<Card> Update(Card card)
    {
        throw new NotImplementedException();
    }
}