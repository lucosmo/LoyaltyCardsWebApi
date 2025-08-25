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
        private readonly IUserService _userService;
        private readonly IDateTimeProvider _dateTimeProvider;
        
        public CardService(ICardRepository cardRepository, IUserService userService, IDateTimeProvider dateTimeProvider)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }
        public async Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard, int? userId)
        {
            var barcodeExists = await _cardRepository.ExistsCardByBarcodeAsync(newCard.Barcode, userId);
            if (barcodeExists)
            {
                return Result<CardDto>.Fail("A card with this barcode already exists for the user.");
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
            var cardResult = await _cardRepository.GetCardByIdAsync(id);
            if (cardResult == null)
            {
                return Result<CardDto>.Fail("Card not found.");
            }
            if (cardResult.UserId != userId)
            {
                return Result<CardDto>.Fail("You do not have permission to delete this card.");
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
            var cardResult = await _cardRepository.GetCardByIdAsync(id);

            if (cardResult is null)
            {
                return Result<CardDto>.Fail("Card not found");
            }
            if (cardResult.UserId != userId)
            {
                return Result<CardDto>.Fail("You do not have permission to access this card.");
            }
            return Result<CardDto>.Ok(cardResult.ToDto());
        }

        public async Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int? userId)
        {
            var cards = await _cardRepository.GetCardsByUserIdAsync(userId);
            if (cards is null || !cards.Any())
            {
                return Result<IEnumerable<CardDto>>.Fail("No cards found for this user.");
            }
            return Result<IEnumerable<CardDto>>.Ok(cards.Select(card => card.ToDto()));
        }

        public async Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto updateCard, int? userId)
        {
            var currentCard = await _cardRepository.GetCardByIdAsync(id);
            if (currentCard is null)
            {
                return Result<CardDto>.Fail("Card not found.");
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
            if (card.UserId != userId)
            {
                return Result<CardDto>.Fail("You do not have permission to access this card.");
            }
            
            var updatedCardResult = await _cardRepository.UpdateCardAsync(card);
            if (updatedCardResult is null)
            {
                return Result<CardDto>.Fail("Card not found or update failed.");
            }
            return Result<CardDto>.Ok(updatedCardResult.ToDto());

        }
    }
}
