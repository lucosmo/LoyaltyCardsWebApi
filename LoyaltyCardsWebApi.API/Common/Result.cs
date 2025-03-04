public class Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(T value)
    {
        Success = true;
        Value = value;
    }

    private Result(string error)
    {
        Success = false;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);
}