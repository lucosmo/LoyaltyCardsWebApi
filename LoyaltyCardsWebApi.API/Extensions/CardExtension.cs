using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;

namespace LoyaltyCardsWebApi.API.Extensions
{
    public static class CardExtension
    {
        public static CardDto ToDto(this Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                Name = card.Name,
                UserId = card.UserId
            };
        }
    }
}
