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
        public CardService(ICardRepository cardRepository, IUserService userService)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        public async Task<Result<CardDto>> CreateCardAsync(CreateCardDto newCard)
        {
            var currentUserResult = await _userService.GetCurrentUserAsync();
            if (!currentUserResult.Success || currentUserResult.Value is null)
            {
                return Result<CardDto>.Fail(currentUserResult.Error ?? "User not found");
            }

            var newCardModel = new Card
            {
                Name = newCard.Name,
                Image = newCard.Image,
                Barcode = newCard.Barcode,
                AddedAt = DateTime.UtcNow,
                UserId = currentUserResult.Value.Id
            };

            var createdCard = await _cardRepository.CreateCardAsync(newCardModel);
            
            if (createdCard is null)
            {
                return Result<CardDto>.Fail("Card creation failed.");
            }
            
            return Result<CardDto>.Ok(createdCard.ToDto());
        }

        public async Task<Result<CardDto>> DeleteCardAsync(int id)
        {
            var currentUserResult = await _userService.GetCurrentUserAsync();
            var cardDeleted = await _cardRepository.Delete(id);
            if (cardDeleted is null)
            {
                return Result<CardDto>.Fail("Card not found or deletion failed.");
            }
            else if(!currentUserResult.Success || currentUserResult.Value is null || cardDeleted.Id != currentUserResult.Value.Id)
            {
                return Result<CardDto>.Fail("You do not have permission to delete this card.");
            }
            else
            {
                return Result<CardDto>.Ok(cardDeleted.ToDto());
            }
        }

        public async Task<Result<CardDto>> GetCardByIdAsync(int id)
        {
            var cardResult = await _cardRepository.GetCardByIdAsync(id);
            var currentUserResult = await _userService.GetCurrentUserAsync();

            if (cardResult is null)
            {
                return Result<CardDto>.Fail("Card not found");
            }
            if (!currentUserResult.Success || currentUserResult.Value is null)
            {
                return Result<CardDto>.Fail(currentUserResult.Error ?? "User not found");
            }
            if (cardResult.UserId != currentUserResult.Value.Id)
            {
                return Result<CardDto>.Fail("You do not have permission to access this card.");
            }
            return Result<CardDto>.Ok(cardResult.ToDto());
        }

        public async Task<Result<IEnumerable<CardDto>>> GetCardsByUserIdAsync(int userId)
        {
            var cards = await _cardRepository.GetCardsByUserIdAsync(userId);
            if (cards is null || !cards.Any())
            {
                return Result<IEnumerable<CardDto>>.Fail("No cards found for this user.");
            }
            return Result<IEnumerable<CardDto>>.Ok(cards.Select(card => card.ToDto()));
        }

        public async Task<Result<CardDto>> UpdateCardAsync(int id, UpdateCardDto updateCard)
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

            var updatedCardResult = await _cardRepository.UpdateCardAsync(card);
            if (updatedCardResult is null)
            {
                return Result<CardDto>.Fail("Card not found or update failed.");
            }
            return Result<CardDto>.Ok(updatedCardResult.ToDto());

        }
    }
}
