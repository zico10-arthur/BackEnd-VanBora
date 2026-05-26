using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IEmailService
{
    Task<Result<bool>> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
