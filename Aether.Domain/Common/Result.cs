namespace Aether.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Fail(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    internal static Result<T> Ok(T value) => new(true, value, Error.None);
    internal static Result<T> Fail(Error error) => new(false, default, error);
}
