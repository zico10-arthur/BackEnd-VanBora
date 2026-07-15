using System.Diagnostics.CodeAnalysis;

namespace VanBora.Domain.Common;

public static class Guard
{
    public static void AgainstNull([NotNull] object? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
    }

    public static void AgainstNullOrWhiteSpace([NotNull] string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
    }

    public static void AgainstNegativeOrZero(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be greater than zero.");
    }

    public static void AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} cannot be negative.");
    }

    public static void AgainstNegativeOrZero(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be greater than zero.");
    }

    public static void AgainstLessThan(int value, int minValue, string paramName)
    {
        if (value < minValue)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be at least {minValue}.");
    }

    public static void AgainstFutureDate(DateTime value, string paramName)
    {
        if (value > DateTime.UtcNow)
            throw new ArgumentException($"{paramName} cannot be in the future.", paramName);
    }

    public static void AgainstPastDate(DateTime value, string paramName)
    {
        if (value < DateTime.UtcNow)
            throw new ArgumentException($"{paramName} cannot be in the past.", paramName);
    }

    public static void AgainstInvalidState(bool condition, string message)
    {
        if (!condition)
            throw new DomainInvariantException(message);
    }

    public static void AgainstEmptyGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
    }
}
