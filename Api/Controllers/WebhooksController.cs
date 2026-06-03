using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly MercadoPagoWebhookHandler _handler;

    public WebhooksController(MercadoPagoWebhookHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Webhook do Mercado Pago (notificação de pagamento Pix / Checkout Pro).
    /// </summary>
    [HttpPost("pix")]
    [AllowAnonymous]
    public Task<IActionResult> Pix(CancellationToken cancellationToken) =>
        _handler.ProcessAsync(Request, cancellationToken);
}
