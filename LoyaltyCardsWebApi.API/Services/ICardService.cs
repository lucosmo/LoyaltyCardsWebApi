using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services
{
    public interface ICardService
    {
        Task<Result<CardDto>> GetCardByIdAsync(int id);
        Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int id);
        Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard);
        Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto card);
        Task<Result<CardDto>> DeleteCardAsync(int id);
    }
}
