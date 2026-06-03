using VanBora.Application.DTOs.Viagens;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IViagemPublicService
{
    Task<Result<List<ViagemPublicaResponse>>> ListarDisponiveisAsync(CancellationToken ct = default);
    Task<Result<ViagemVanDetalheResponse>> ObterDetalheViagemVanAsync(Guid viagemVanId, CancellationToken ct = default);
}
