using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;

namespace LoyalityCardsWebApi.API.Services;
public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;

    public CardService(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<Card> CreateCardAsync(CreateCardDto newCard)
    {
        var createdCard = await _cardRepository.CreateAsync();
        return createdCard;
    }

    public async Task<Card?> DeleteAsync(int id)
    {
        var deletedCard = await _cardRepository.DeleteAsync(id);
        return deletedCard;
    }

    public Task<List<Card>?> GetAllCardsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Card?> GetCardByIdAsync(int id)
    {
        var card = await _cardRepository.GetCardByIdAsync(id);
        return card;
    }

    public Task<bool> UpdateCardAsync(int id, UpdatedCardDto updatedCard)
    {
        throw new NotImplementedException();
    }
}