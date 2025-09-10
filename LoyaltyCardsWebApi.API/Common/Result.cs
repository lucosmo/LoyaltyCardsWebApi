using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Common;
public class Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    private Result(bool success, T? value, string error, ErrorType errorType)
    {
        Success = success;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Ok(T value) => new(true, value, string.Empty, ErrorType.None);
    public static Result<T> NotFound(string error) => new(false, default, error, ErrorType.NotFound);
    public static Result<T> BadRequest(string error) => new(false, default, error, ErrorType.BadRequest);
    public static Result<T> Unauthorized(string error) => new(false, default, error, ErrorType.Unauthorized);
    public static Result<T> Forbidden(string error) => new(false, default, error, ErrorType.Forbidden);
    public static Result<T> Conflict(string error) => new(false, default, error, ErrorType.Conflict);
    public static Result<T> Fail(string error) => new(false, default, error, ErrorType.InternalError);
}