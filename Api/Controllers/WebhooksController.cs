using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VanBora.Application.Interfaces;
using VanBora.Application.Settings;
using VanBora.Infrastructure.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly MercadoPagoSettings _mpSettings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IReservaService reservaService,
        IOptions<MercadoPagoSettings> mpSettings,
        IWebHostEnvironment env,
        ILogger<WebhooksController> logger)
    {
        _reservaService = reservaService;
        _mpSettings = mpSettings.Value;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Webhook do Mercado Pago (notificação de pagamento Pix / Checkout Pro).
    /// </summary>
    [HttpPost("pix")]
    [AllowAnonymous]
    public async Task<IActionResult> Pix(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        _logger.LogInformation("Webhook Mercado Pago recebido: {Body}", body);

        var resourceId = MercadoPagoPagamentoGateway.ExtrairIdNotificacao(body, Request.Query["id"].FirstOrDefault())
                         ?? Request.Query["data.id"].FirstOrDefault();

        if (string.IsNullOrEmpty(resourceId))
        {
            _logger.LogWarning("Webhook sem id de pagamento");
            return BadRequest();
        }

        var secret = ResolveWebhookSecret();
        if (!string.IsNullOrWhiteSpace(secret))
        {
            var xSignature = Request.Headers["x-signature"].FirstOrDefault();
            var xRequestId = Request.Headers["x-request-id"].FirstOrDefault();
            if (!MercadoPagoWebhookSignatureValidator.TryValidate(xSignature, xRequestId, resourceId, secret))
            {
                _logger.LogWarning("Assinatura de webhook inválida para data.id={DataId}", resourceId);
                return Unauthorized();
            }
        }
        else if (!_env.IsDevelopment())
        {
            _logger.LogWarning("MercadoPago:WebhookSecret não configurado em ambiente não-dev");
        }

        var result = await _reservaService.ProcessarWebhookPagamentoAsync(resourceId, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogWarning("Falha ao processar webhook: {Code} {Message}", result.Error.Code, result.Error.Message);
            return result.Error.Type switch
            {
                VanBora.Domain.Common.ErrorType.NotFound => NotFound(),
                VanBora.Domain.Common.ErrorType.Validation => BadRequest(new { result.Error.Code, result.Error.Message }),
                _ => StatusCode(500)
            };
        }

        return Ok();
    }

    private string? ResolveWebhookSecret()
    {
        if (!string.IsNullOrWhiteSpace(_mpSettings.WebhookSecret))
            return _mpSettings.WebhookSecret;

        return Environment.GetEnvironmentVariable("MP_WEBHOOK_SECRET");
    }
}
