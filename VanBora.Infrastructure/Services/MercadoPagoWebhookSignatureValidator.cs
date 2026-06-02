using System.Security.Cryptography;
using System.Text;

namespace VanBora.Infrastructure.Services;

/// <summary>
/// Validação da assinatura HMAC do webhook Mercado Pago (header x-signature).
/// </summary>
public static class MercadoPagoWebhookSignatureValidator
{
    public static bool TryValidate(
        string? xSignatureHeader,
        string? xRequestId,
        string dataId,
        string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(webhookSecret) ||
            string.IsNullOrWhiteSpace(xSignatureHeader) ||
            string.IsNullOrWhiteSpace(dataId))
            return false;

        if (!TryParseSignatureHeader(xSignatureHeader, out var ts, out var v1) ||
            string.IsNullOrEmpty(ts) ||
            string.IsNullOrEmpty(v1))
            return false;

        var manifest = $"id:{dataId};request-id:{xRequestId ?? ""};ts:{ts};";
        var computed = ComputeHmacHex(webhookSecret, manifest);
        return FixedTimeEquals(computed, v1);
    }

    private static bool TryParseSignatureHeader(string header, out string? ts, out string? v1)
    {
        ts = null;
        v1 = null;
        foreach (var segment in header.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = segment.IndexOf('=');
            if (eq <= 0) continue;
            var key = segment[..eq];
            var value = segment[(eq + 1)..];
            if (key == "ts") ts = value;
            if (key == "v1") v1 = value;
        }

        return ts is not null && v1 is not null;
    }

    private static string ComputeHmacHex(string secret, string manifest)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(manifest);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        if (ba.Length != bb.Length) return false;
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
