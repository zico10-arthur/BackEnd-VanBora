namespace VanBora.Domain.Common;

/// <summary>
/// Base exception for domain-specific errors that should expose their
/// message even in production environments.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
