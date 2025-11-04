namespace LoyaltyCardsWebApi.API.Common;
public class Result<T>
{
    public bool Success { get; }
    public SuccessTypes? SuccessType { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorTypes ErrorType { get; }
    public string? Location { get; } = null;

    private Result(bool success, SuccessTypes? successType, T? value, string error, ErrorTypes errorType, string? location = null)
    {
        Success = success;
        SuccessType = successType;
        Value = value;
        Error = error;
        ErrorType = errorType;
        Location = location;
    }

    public static Result<T> Ok(T value) => new(true, SuccessTypes.Ok, value, string.Empty, ErrorTypes.None);
    public static Result<T> Created(T value, string location) => new(true, SuccessTypes.Created, value, string.Empty, ErrorTypes.None, location);
    public static Result<T> NoContent() => new(true, SuccessTypes.NoContent, default, string.Empty, ErrorTypes.None);
    public static Result<T> NotFound(string error) => new(false, null, default, error, ErrorTypes.NotFound);
    public static Result<T> BadRequest(string error) => new(false, null, default, error, ErrorTypes.BadRequest);
    public static Result<T> Unauthorized(string error) => new(false, null, default, error, ErrorTypes.Unauthorized);
    public static Result<T> Forbidden(string error) => new(false, null, default, error, ErrorTypes.Forbidden);
    public static Result<T> Conflict(string error) => new(false, null, default, error, ErrorTypes.Conflict);
    public static Result<T> Fail(string error) => new(false, null, default, error, ErrorTypes.InternalError);
}