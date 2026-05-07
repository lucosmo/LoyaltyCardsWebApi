using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using Serilog.Data;

namespace LoyaltyCardsWebApi.API.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<CardService> _logger;

        public CardService(ICardRepository cardRepository, IDateTimeProvider dateTimeProvider, ILogger<CardService> logger)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard, int? userId, CancellationToken cancellationToken = default)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to create a card.");
            }
            
            var barcodeExists = await _cardRepository.ExistsCardByBarcodeAsync(newCard.Barcode, userId.Value, cancellationToken);
            if (barcodeExists)
            {
                _logger.LogWarning("Create card failed: Card already exists for User {UserId}. Barcode: {Barcode}", userId.Value, newCard.Barcode);
                return Result<CardDto>.Conflict("A card with this barcode already exists for the user.");
            }

            var newCardModel = new Card
            {
                Name = newCard.Name,
                Image = newCard.Image,
                Barcode = newCard.Barcode,
                AddedAt = _dateTimeProvider.UtcNow,
                UserId = (int)userId
            };

            var createdCard = await _cardRepository.CreateCardAsync(newCardModel, cancellationToken);

            if (createdCard is null)
            {
                _logger.LogError("System Error: Failed to persist new card for User {UserId}.", userId);
                return Result<CardDto>.Fail("Card creation failed.");
            }

            _logger.LogInformation("Card created. Id: {CardId}, User: {UserId}.", createdCard.Id, userId);
            return Result<CardDto>.Ok(createdCard.ToDto());
        }

        public async Task<Result<CardDto>> DeleteCardAsync(int id, int? userId, CancellationToken cancellationToken = default)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to delete this card.");
            }
            var cardDeleted = await _cardRepository.DeleteAsync(id, userId.Value, cancellationToken);
            if (cardDeleted is null)
            {
                _logger.LogError("System Error: Failed to delete Card {CardId}", id);
                return Result<CardDto>.Fail("Deletion failed.");
            }

            _logger.LogInformation("Card {CardId} deleted by User {UserId}.", id, userId);
            return Result<CardDto>.Ok(cardDeleted.ToDto());
        }

        public async Task<Result<CardDto>> GetCardByIdAsync(int id, int? userId, CancellationToken cancellationToken = default)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to access this card.");
            }
            var cardResult = await _cardRepository.GetCardByIdAsync(id, userId.Value, cancellationToken); 
            if (cardResult is null)
            {
                return Result<CardDto>.NotFound("Card not found.");
            }
            if (cardResult.UserId != userId)
            {
                _logger.LogWarning("Security Alert: User {CurrentUserId} tried to access Card {CardId} belonging to User {TargetUserId}.", userId, id, cardResult.UserId);
                return Result<CardDto>.Forbidden("You do not have permission to access this card.");
            }
            return Result<CardDto>.Ok(cardResult.ToDto());
        }

        public async Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int userId, int? currentUserId, CancellationToken cancellationToken = default)
        {
            if (!currentUserId.HasValue)
            {
                return Result<IEnumerable<CardDto>>.Unauthorized("Authentication is required.");
            }
            if (currentUserId.Value != userId)
            {
                _logger.LogWarning("Security Alert: User {CurrentUserId} tried to access cards of User {TargetUserId}.", currentUserId, userId);
                return Result<IEnumerable<CardDto>>.Forbidden("No permission.");
            }
            var cards = await _cardRepository.GetCardsByUserIdAsync(userId, cancellationToken);
            return Result<IEnumerable<CardDto>>.Ok(cards.Select(card => card.ToDto()));
        }

        public async Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto updateCard, int? userId, CancellationToken cancellationToken = default)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to update this card.");
            }
            var currentCard = await _cardRepository.GetCardByIdAsync(id, userId.Value, cancellationToken);
            if (currentCard is null)
            {
                _logger.LogWarning("Update failed: Card {CardId} not found for User {UserId}.", id, userId);
                return Result<CardDto>.NotFound("Card not found.");
            }
            if (currentCard.UserId != userId)
            {
                _logger.LogWarning("Security Alert: User {CurrentUserId} tried to update Card {CardId} belonging to User {TargetUserId}.", userId, id, currentCard.UserId);
                return Result<CardDto>.Forbidden("No permission.");
            }
           
            var updatedCardResult = await _cardRepository.UpdateCardAsync(id, updateCard, userId.Value, cancellationToken);
            if (updatedCardResult is null)
            {
                _logger.LogError("System Error: Update failed for Card {CardId}, User {UserId}.", id, userId);
                return Result<CardDto>.Fail("Card not found or update failed.");
            }

            _logger.LogInformation("Card {CardId} updated by User {UserId}.", id, userId);
            return Result<CardDto>.Ok(updatedCardResult.ToDto());
        }
    }
}
