using VanBora.Application.DTOs.Viagens;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IViagemService
{
    Task<Result<ViagemResponse>> CriarAsync(Guid gerenteUsuarioId, CriarViagemRequest request, CancellationToken cancellationToken = default);
    Task<Result<ViagemResponse>> AtualizarAsync(Guid gerenteUsuarioId, Guid viagemId, AtualizarViagemRequest request, CancellationToken cancellationToken = default);
    Task<Result<ViagemResponse>> ObterPorIdAsync(Guid viagemId, CancellationToken cancellationToken = default);
    Task<Result<ViagemGerenteResponse>> ObterPorGerenteAsync(Guid gerenteUsuarioId, Guid viagemId, CancellationToken cancellationToken = default);
    Task<Result<List<ViagemResponse>>> ListarDisponiveisAsync(CancellationToken cancellationToken = default);
    Task<Result<List<ViagemGerenteResponse>>> ListarPorGerenteAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task<Result<ViagemResponse>> AlocarVanAsync(Guid gerenteUsuarioId, Guid viagemId, AlocarVanRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoverVanAsync(Guid gerenteUsuarioId, Guid viagemId, Guid viagemVanId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelarAsync(Guid gerenteUsuarioId, Guid viagemId, CancellationToken cancellationToken = default);

     Task<Result<ViagemResponse>> AlocarMotoristaAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        AlocarMotoristaRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoverMotoristaAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        Guid viagemVanId,
        CancellationToken cancellationToken = default);
}
