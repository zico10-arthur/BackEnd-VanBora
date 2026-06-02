using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VanBora.Application.Interfaces;
using VanBora.Application.Settings;
using VanBora.Domain.Common;
using VanBora.Infrastructure.Services;

namespace Api.Services;

/// <summary>
/// Processamento compartilhado de webhooks Mercado Pago (validação + confirmação de reserva).
/// </summary>
public sealed class MercadoPagoWebhookHandler
{
    private readonly IReservaService _reservaService;
    private readonly MercadoPagoSettings _mpSettings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MercadoPagoWebhookHandler> _logger;

    public MercadoPagoWebhookHandler(
        IReservaService reservaService,
        IOptions<MercadoPagoSettings> mpSettings,
        IWebHostEnvironment env,
        ILogger<MercadoPagoWebhookHandler> logger)
    {
        _reservaService = reservaService;
        _mpSettings = mpSettings.Value;
        _env = env;
        _logger = logger;
    }

    public async Task<IActionResult> ProcessAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var resourceId = MercadoPagoPagamentoGateway.ExtrairIdNotificacao(body, request.Query["id"].FirstOrDefault())
                         ?? request.Query["data.id"].FirstOrDefault();

        var requestId = request.Headers["x-request-id"].FirstOrDefault();

        if (string.IsNullOrEmpty(resourceId))
        {
            _logger.LogWarning("Webhook sem id de pagamento (request-id={RequestId})", requestId);
            return new BadRequestResult();
        }

        _logger.LogInformation(
            "Webhook Mercado Pago recebido: resourceId={ResourceId}, request-id={RequestId}, bodyLen={BodyLen}",
            resourceId,
            requestId,
            body.Length);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Webhook body: {Body}", body);

        var secret = ResolveWebhookSecret();
        if (string.IsNullOrWhiteSpace(secret))
        {
            if (!_env.IsDevelopment())
            {
                _logger.LogError("MercadoPago:WebhookSecret não configurado em ambiente não-dev");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            var xSignature = request.Headers["x-signature"].FirstOrDefault();
            if (!MercadoPagoWebhookSignatureValidator.TryValidate(xSignature, requestId, resourceId, secret))
            {
                _logger.LogWarning("Assinatura de webhook inválida para data.id={DataId}", resourceId);
                return new UnauthorizedResult();
            }
        }

        var result = await _reservaService.ProcessarWebhookPagamentoAsync(resourceId, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Falha ao processar webhook: {Code} {Message}", result.Error.Code, result.Error.Message);
            return result.Error.Type switch
            {
                ErrorType.NotFound => new NotFoundResult(),
                ErrorType.Validation => new BadRequestObjectResult(new { result.Error.Code, result.Error.Message }),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }

        return new OkResult();
    }

    private string? ResolveWebhookSecret()
    {
        if (!string.IsNullOrWhiteSpace(_mpSettings.WebhookSecret))
            return _mpSettings.WebhookSecret;

        return Environment.GetEnvironmentVariable("MP_WEBHOOK_SECRET");
    }
}
