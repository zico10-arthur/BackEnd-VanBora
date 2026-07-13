using VanBora.Application.DTOs.Vans;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IVanService
{
    Task<Result<VanResponse>> CriarAsync(Guid gerenteUsuarioId, CriarVanRequest request, CancellationToken cancellationToken = default);
    Task<Result<VanResponse>> AtualizarAsync(Guid gerenteUsuarioId, Guid vanId, AtualizarVanRequest request, CancellationToken cancellationToken = default);
    Task<Result<VanResponse>> ObterPorIdAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default);
    Task<Result<List<VanResponse>>> ListarPorGerenteAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoverAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default);
    Task<Result<VanResponse>> AlternarStatusAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default);
}
