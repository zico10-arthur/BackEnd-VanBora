namespace VanBora.Application.Settings;

public class MercadoPagoSettings
{
    public const string SectionName = "MercadoPago";

    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Opcional — Checkout Pro atual usa só Access Token no servidor.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>URL pública do webhook (POST). Ex.: https://api.seudominio.com/api/webhooks/pix</summary>
    public string NotificationUrl { get; set; } = string.Empty;

    /// <summary>Segredo de assinatura do webhook (painel MP → Webhooks). Opcional em dev.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = "http://localhost:3000/reserva/sucesso";
    public string FailureUrl { get; set; } = "http://localhost:3000/reserva/falha";
    public string PendingUrl { get; set; } = "http://localhost:3000/reserva/pendente";
}
