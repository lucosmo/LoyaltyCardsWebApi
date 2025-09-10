namespace LoyaltyCardsWebApi.API.Common;

public enum ErrorType
{
    None,
    NotFound,
    BadRequest,
    Unauthorized,
    Forbidden,
    Conflict,
    InternalError
}