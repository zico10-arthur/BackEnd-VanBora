using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Infrastructure.Configuration;

namespace VanBora.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning(
                "[EmailService Mock] EmailSettings não configurado. " +
                "Usando mock. Destinatário: {To} | Assunto: {Subject}",
                to, subject);
            _logger.LogInformation(
                "[EmailService Mock] Corpo: {Body}", body);
            return Result<bool>.Success(true);
        }

        if (string.IsNullOrWhiteSpace(to) || !IsValidEmail(to))
        {
            return Error.Failure("EMAIL_INVALIDO", "Endereço de email inválido.");
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);

            using var client = new SmtpClient
            {
                Host = _settings.Host,
                Port = _settings.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                Timeout = 10_000
            };

            await client.SendMailAsync(message, cancellationToken);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation(
                "[EmailService] Email enviado com sucesso para: {To} | Assunto: {Subject}",
                to, subject);
            return Result<bool>.Success(true);
        }
        catch (SmtpException ex) when (ex.StatusCode == SmtpStatusCode.ClientNotPermitted)
        {
            _logger.LogError(ex, "[EmailService] Credenciais SMTP inválidas para {Host}", _settings.Host);
            return Error.Failure("EMAIL_CREDENCIAIS_INVALIDAS", "Credenciais de email inválidas.");
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "[EmailService] Falha SMTP ao enviar para {To}", to);
            return Error.Failure("EMAIL_FALHA_SMTP", "Falha ao conectar ao servidor de email.");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("[EmailService] Envio cancelado para {To}", to);
            return Error.Failure("EMAIL_CANCELADO", "Operação cancelada.");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("[EmailService] Timeout ao enviar email para {To}", to);
            return Error.Failure("EMAIL_TIMEOUT", "Tempo limite excedido ao enviar email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailService] Erro inesperado ao enviar email para {To}", to);
            return Error.Failure("EMAIL_ERRO_INTERNO", "Erro ao enviar email.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
