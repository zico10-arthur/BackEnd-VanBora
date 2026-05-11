using System.Diagnostics.CodeAnalysis;

namespace VanBora.Domain.Common;

public class Result<T> : IAppResult
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }
    public bool HasValue => true;

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public T GetValueOrThrow() =>
        IsFailure
            ? throw new InvalidOperationException(
                $"Cannot access value. Error: {Error.Message}")
            : Value;

    object? IAppResult.GetValue() => Value;

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public class Result : IAppResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public bool HasValue => false;

    private Result()
    {
        IsSuccess = true;
        Error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);

    public void GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException(
                $"Operation failed. Error: {Error.Message}");
    }

    object? IAppResult.GetValue() => null;

    public static implicit operator Result(Error error) => Failure(error);
}
