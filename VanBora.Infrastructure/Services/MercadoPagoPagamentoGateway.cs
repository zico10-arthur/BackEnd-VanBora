using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VanBora.Application.Interfaces;
using VanBora.Application.Settings;
using VanBora.Domain.Common;

namespace VanBora.Infrastructure.Services;

public class MercadoPagoPagamentoGateway : IPagamentoGateway
{
    private readonly HttpClient _http;
    private readonly MercadoPagoSettings _settings;

    public MercadoPagoPagamentoGateway(HttpClient http, IOptions<MercadoPagoSettings> settings)
    {
        _http = http;
        _settings = settings.Value;

        var token = _settings.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            token = Environment.GetEnvironmentVariable("MP_ACCESS_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        if (_http.BaseAddress is null)
            _http.BaseAddress = new Uri("https://api.mercadopago.com/");
    }

    public async Task<Result<PreferenciaPagamentoResult>> CriarPreferenciaAsync(
        Guid reservaId,
        string titulo,
        decimal valorTotal,
        DateTime expiraEmUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AccessToken) &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MP_ACCESS_TOKEN")))
        {
            return Error.Failure(
                "MP_NAO_CONFIGURADO",
                "Configure MercadoPago:AccessToken ou a variável MP_ACCESS_TOKEN.");
        }

        var agora = DateTime.UtcNow;
        var payload = new
        {
            items = new[]
            {
                new
                {
                    id = reservaId.ToString("N"),
                    title = titulo,
                    quantity = 1,
                    currency_id = "BRL",
                    unit_price = valorTotal
                }
            },
            external_reference = reservaId.ToString(),
            notification_url = string.IsNullOrWhiteSpace(_settings.NotificationUrl)
                ? null
                : _settings.NotificationUrl,
            back_urls = new
            {
                success = AppendReservaId(_settings.SuccessUrl, reservaId),
                failure = AppendReservaId(_settings.FailureUrl, reservaId),
                pending = AppendReservaId(_settings.PendingUrl, reservaId)
            },
            auto_return = "approved",
            expires = true,
            expiration_date_from = agora.ToString("o"),
            expiration_date_to = expiraEmUtc.ToString("o"),
            payment_methods = new
            {
                excluded_payment_types = new[]
                {
                    new { id = "credit_card" },
                    new { id = "debit_card" },
                    new { id = "ticket" }
                }
            }
        };

        var resp = await _http.PostAsJsonAsync("checkout/preferences", payload, cancellationToken);
        var body = await resp.Content.ReadAsStringAsync(cancellationToken);

        if (!resp.IsSuccessStatusCode)
            return Error.Failure("MP_PREFERENCIA_FALHOU", $"Mercado Pago: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var id = root.GetProperty("id").GetString() ?? string.Empty;
        var initPoint = root.TryGetProperty("init_point", out var ip)
            ? ip.GetString() ?? string.Empty
            : string.Empty;
        var sandbox = root.TryGetProperty("sandbox_init_point", out var sip)
            ? sip.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(initPoint))
            return Error.Failure("MP_PREFERENCIA_INVALIDA", "Resposta sem init_point do Mercado Pago.");

        return Result<PreferenciaPagamentoResult>.Success(
            new PreferenciaPagamentoResult(id, initPoint, sandbox));
    }

    public async Task<Result<PagamentoConfirmadoInfo>> ObterPagamentoAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var resp = await _http.GetAsync($"v1/payments/{paymentId}", cancellationToken);
        if (!resp.IsSuccessStatusCode)
            return Error.NotFound("MP_PAGAMENTO_NAO_ENCONTRADO", "Pagamento não encontrado no Mercado Pago.");

        var body = await resp.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var status = root.GetProperty("status").GetString() ?? string.Empty;
        var externalRef = root.TryGetProperty("external_reference", out var ext)
            ? ext.GetString()
            : null;

        return Result<PagamentoConfirmadoInfo>.Success(
            new PagamentoConfirmadoInfo(paymentId, status, externalRef));
    }

    public static string? ExtrairIdNotificacao(string body, string? queryId)
    {
        if (!string.IsNullOrWhiteSpace(queryId))
            return queryId;

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.ValueKind == JsonValueKind.Object &&
                data.TryGetProperty("id", out var idEl))
                return idEl.GetString();

            if (doc.RootElement.TryGetProperty("id", out var idEl2))
                return idEl2.GetString();
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static string AppendReservaId(string baseUrl, Guid reservaId)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return baseUrl;

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}reservaId={Uri.EscapeDataString(reservaId.ToString())}";
    }
}
