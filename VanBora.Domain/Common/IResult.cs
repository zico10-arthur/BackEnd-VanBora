namespace VanBora.Domain.Common;

/// <summary>
/// Defines the common contract for both <see cref="Result{T}"/> and <see cref="Result"/>.
/// Enables pattern matching in filters and middlewares without reflection.
/// </summary>
public interface IAppResult
{
    /// <summary>Indicates whether the operation completed successfully.</summary>
    bool IsSuccess { get; }

    /// <summary>Indicates whether the operation failed.</summary>
    bool IsFailure { get; }

    /// <summary>The error details when the operation fails.</summary>
    Error Error { get; }

    /// <summary>Whether this result carries a value (true for <see cref="Result{T}"/>, false for void <see cref="Result"/>).</summary>
    bool HasValue { get; }

    /// <summary>Returns the inner value when available, or null for void results.</summary>
    object? GetValue();
}
