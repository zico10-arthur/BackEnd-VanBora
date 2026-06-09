using VanBora.Application.DTOs.Admin;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IAdminService
{
    // US13 — Gestão de Gerentes
    Task<Result<List<GerenteAdminResponse>>> ListarGerentesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> ObterGerentePorIdAsync(
        Guid gerenteId,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> CriarGerenteAsync(
        CriarGerenteAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> AtualizarGerenteAsync(
        Guid gerenteId,
        AtualizarGerenteAdminRequest request,
        CancellationToken cancellationToken = default);

    // US22 — Buscar Usuarios
    Task<Result<List<UsuarioAdminResponse>>> BuscarUsuariosAsync(
        string? search,
        CancellationToken cancellationToken = default);

    // US23 — Histórico de Reservas
    Task<Result<List<ReservaHistoricoResponse>>> ObterHistoricoReservasUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<Result<List<ViagemGerenteHistoricoResponse>>> ObterHistoricoReservasGerenteAsync(
        Guid gerenteId,
        CancellationToken cancellationToken = default);
}
