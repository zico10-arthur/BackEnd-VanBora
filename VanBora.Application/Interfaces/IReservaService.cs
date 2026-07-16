using VanBora.Application.DTOs.Reservas;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IReservaService
{
    Task<Result<ReservaResponse>> CriarReservaAsync(
        Guid usuarioId,
        CriarReservaRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<List<ReservaResponse>>> ListarMinhasReservasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<Result<ReservaResponse>> ObterReservaPorIdAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default);

    Task<Result<PagarReservaResponse>> PagarReservaAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default);

    Task<Result<ReservaResponse>> CancelarReservaAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default);

    Task<Result<ContatoGerenteResponse>> ObterContatoGerenteAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default);

    Task<Result> ProcessarWebhookPagamentoAsync(
        string paymentId,
        CancellationToken cancellationToken = default);

    Task ExpirarReservasPendentesAsync(
        CancellationToken cancellationToken = default);
}
