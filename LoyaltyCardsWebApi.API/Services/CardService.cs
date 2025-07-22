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
            return Result<CardDto>.Ok(createdCard.ToDto());
        }

        public Task<Result<Card>> DeleteCardAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Card>> GetCardByIdAsync(int id)
        {
            throw new NotImplementedException();
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

        public Task<Result<CardDto>> UpdateCardAsync(int id, UpdatedCardDto card)
        {
            throw new NotImplementedException();
        }
    }
}
