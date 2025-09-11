using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services
{
    public interface ICardService
    {
        Task<Result<CardDto>> GetCardByIdAsync(int id, int? userId);
        Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int? id);
        Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard, int? userId);
        Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto card, int? userId);
        Task<Result<CardDto>> DeleteCardAsync(int id, int? userId);
    }
}
