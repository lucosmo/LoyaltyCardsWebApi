namespace LoyaltyCardsWebApi.API.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
