using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Services
{
    public interface ICardService
    {
        Task<Result<CardDto>> GetCardByIdAsync(int id, int? userId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int? userId, int? currentUserId, CancellationToken cancellationToken = default);
        Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard, int? userId, CancellationToken cancellationToken = default);
        Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto card, int? userId, CancellationToken cancellationToken = default);
        Task<Result<CardDto>> DeleteCardAsync(int id, int? userId, CancellationToken cancellationToken = default);
    }
}
