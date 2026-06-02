using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Alias legado para webhook Mercado Pago (<c>/api/payments/webhook</c>).
/// Prefira <see cref="WebhooksController"/> em <c>/api/webhooks/pix</c>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly MercadoPagoWebhookHandler _handler;

    public PaymentsController(MercadoPagoWebhookHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public Task<IActionResult> Webhook(CancellationToken cancellationToken) =>
        _handler.ProcessAsync(Request, cancellationToken);
}
