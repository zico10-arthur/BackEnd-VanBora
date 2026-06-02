using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public sealed record PreferenciaPagamentoResult(
    string PreferenceId,
    string InitPoint,
    string? SandboxInitPoint);

public interface IPagamentoGateway
{
    Task<Result<PreferenciaPagamentoResult>> CriarPreferenciaAsync(
        Guid reservaId,
        string titulo,
        decimal valorTotal,
        DateTime expiraEmUtc,
        CancellationToken cancellationToken = default);

    Task<Result<PagamentoConfirmadoInfo>> ObterPagamentoAsync(
        string paymentId,
        CancellationToken cancellationToken = default);
}

public sealed record PagamentoConfirmadoInfo(
    string PaymentId,
    string Status,
    string? ExternalReference);
