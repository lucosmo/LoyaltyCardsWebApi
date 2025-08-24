using System.Security.Claims;
namespace LoyaltyCardsWebApi.API.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                return null;
            }
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        }

        public static int? GetUserIdAsInt(this ClaimsPrincipal user)
        {
            var userIdString = user.GetUserId();
            if (string.IsNullOrEmpty(userIdString))
            {
                return null;
            }
            if (int.TryParse(userIdString, out int userIdInt))
            {
                return userIdInt;
            }
            else
            {
                return null;
            }    
        }
    }
}