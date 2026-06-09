using Microsoft.Extensions.Logging;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;

namespace VanBora.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<Result<bool>> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EmailService Mock] Enviando email para: {To} | Assunto: {Subject} | Corpo: {Body}",
            to,
            subject,
            body);

        return Task.FromResult(Result<bool>.Success(true));
    }
}
