using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.Interfaces;
using VanBora.Infrastructure.Services;

namespace Api.Controllers;

/// <summary>
/// Alias legado para webhook Mercado Pago (<c>/api/payments/webhook</c>).
/// Prefira <see cref="WebhooksController"/> em <c>/api/webhooks/pix</c>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IReservaService reservaService, ILogger<PaymentsController> logger)
    {
        _reservaService = reservaService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        _logger.LogInformation("MercadoPago webhook (payments): {Body}", body);

        var resourceId = MercadoPagoPagamentoGateway.ExtrairIdNotificacao(body, Request.Query["id"].FirstOrDefault());
        if (string.IsNullOrEmpty(resourceId))
            return BadRequest();

        var result = await _reservaService.ProcessarWebhookPagamentoAsync(resourceId, cancellationToken);
        return result.IsFailure ? StatusCode(500) : Ok();
    }
}
