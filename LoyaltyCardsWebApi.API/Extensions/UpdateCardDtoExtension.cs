using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Extensions
{
    public static class UpdateCardDtoExtension
    {
        public static Card ToCard(this UpdateCardDto updateCardDto, int id, int userId, DateTime addedAt)
        {
            
            return new Card
            {
                Id = id,
                Name = updateCardDto.Name,
                Image = updateCardDto.Image,
                Barcode = updateCardDto.Barcode,
                AddedAt = addedAt,
                UserId = userId
            };
        }
    }
}
