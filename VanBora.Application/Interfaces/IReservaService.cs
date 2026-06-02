using VanBora.Application.DTOs.Reservas;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IReservaService
{
    Task<Result<ReservaResponse>> CriarAsync(Guid usuarioId, CriarReservaRequest request, CancellationToken ct = default);
    Task<Result<PagarReservaResponse>> GerarPagamentoAsync(Guid usuarioId, Guid reservaId, CancellationToken ct = default);
    Task<Result<ReservaResponse>> ObterPorIdAsync(Guid usuarioId, Guid reservaId, CancellationToken ct = default);
    Task<Result<List<ReservaResponse>>> ListarMinhasAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Result<ReservaResponse>> CancelarAsync(Guid usuarioId, Guid reservaId, CancellationToken ct = default);
    Task<Result> ProcessarWebhookPagamentoAsync(string paymentId, CancellationToken ct = default);
    Task ExpirarReservasPendentesAsync(CancellationToken ct = default);
}
