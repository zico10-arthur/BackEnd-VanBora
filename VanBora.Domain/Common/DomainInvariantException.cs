namespace VanBora.Domain.Common;

public sealed class DomainInvariantException : DomainException
{
    public DomainInvariantException(string message) : base(message) { }
}
