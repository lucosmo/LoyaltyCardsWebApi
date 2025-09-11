using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;

namespace LoyaltyCardsWebApi.API.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        
        public CardService(ICardRepository cardRepository, IDateTimeProvider dateTimeProvider)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }
        public async Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard, int? userId)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to create a card.");
            }
            var barcodeExists = await _cardRepository.ExistsCardByBarcodeAsync(newCard.Barcode, userId);
            if (barcodeExists)
            {
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

            var createdCard = await _cardRepository.CreateCardAsync(newCardModel);

            if (createdCard is null)
            {
                return Result<CardDto>.Fail("Card creation failed.");
            }

            return Result<CardDto>.Ok(createdCard.ToDto());
        }

        public async Task<Result<CardDto>> DeleteCardAsync(int id, int? userId)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to delete this card.");
            }
            var cardResult = await _cardRepository.GetCardByIdAsync(id);
            if (cardResult is null)
            {
                return Result<CardDto>.NotFound("Card not found.");
            }
            if (cardResult.UserId != userId)
            {
                return Result<CardDto>.Forbidden("You do not have permission to delete this card.");
            }
            var cardDeleted = await _cardRepository.Delete(id);
            if (cardDeleted is null)
            {
                return Result<CardDto>.Fail("Deletion failed.");
            }
            return Result<CardDto>.Ok(cardDeleted.ToDto());
        }

        public async Task<Result<CardDto>> GetCardByIdAsync(int id, int? userId)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to access this card.");
            }
            var cardResult = await _cardRepository.GetCardByIdAsync(id); 
            if (cardResult is null)
            {
                return Result<CardDto>.NotFound("Card not found.");
            }
            if (cardResult.UserId != userId)
            {
                return Result<CardDto>.Forbidden("You do not have permission to access this card.");
            }
            return Result<CardDto>.Ok(cardResult.ToDto());
        }

        public async Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int? userId)
        {
            if (userId is null)
            {
                return Result<IEnumerable<CardDto>>.BadRequest("User ID is required to access cards.");
            }
            var cards = await _cardRepository.GetCardsByUserIdAsync(userId);
            return Result<IEnumerable<CardDto>>.Ok(cards.Select(card => card.ToDto()));
        }

        public async Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto updateCard, int? userId)
        {
            if (userId is null)
            {
                return Result<CardDto>.BadRequest("User ID is required to update this card.");
            }

            var currentCard = await _cardRepository.GetCardByIdAsync(id);
            if (currentCard is null)
            {
                return Result<CardDto>.NotFound("Card not found.");
            }
            
            if (currentCard.UserId != userId)
            {
                return Result<CardDto>.Forbidden("You do not have permission to access this card.");
            }

            Card card = new Card
            {
                Id = id,
                Name = updateCard.Name ?? currentCard.Name,
                Image = updateCard.Image ?? currentCard.Image,
                Barcode = updateCard.Barcode ?? currentCard.Barcode,
                UserId = currentCard.UserId, 
                AddedAt = currentCard.AddedAt 
            };
            
            var updatedCardResult = await _cardRepository.UpdateCardAsync(card);
            if (updatedCardResult is null)
            {
                return Result<CardDto>.Fail("Card not found or update failed.");
            }
            return Result<CardDto>.Ok(updatedCardResult.ToDto());

        }
    }
}
