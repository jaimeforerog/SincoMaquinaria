namespace SincoMaquinaria.Services;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public bool IsNotFound { get; }
    public bool IsUnauthorized { get; }
    public bool IsConflict { get; }

    private Result(bool isSuccess, T? value, string? error, bool isNotFound = false, bool isUnauthorized = false, bool isConflict = false)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        IsNotFound = isNotFound;
        IsUnauthorized = isUnauthorized;
        IsConflict = isConflict;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
    public static Result<T> NotFound(string error) => new Result<T>(false, default, error, true);
    public static Result<T> Unauthorized() => new Result<T>(false, default, "Unauthorized", false, true);
    public static Result<T> Conflict(string error) => new Result<T>(false, default, error, isConflict: true);
}

public struct Unit
{
    public static readonly Unit Value = new Unit();
}
