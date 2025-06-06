using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Extensions;

public static class UserExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };
    }
}