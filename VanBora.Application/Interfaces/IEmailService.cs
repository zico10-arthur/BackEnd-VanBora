using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IEmailService
{
    Task<Result<bool>> SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
}
