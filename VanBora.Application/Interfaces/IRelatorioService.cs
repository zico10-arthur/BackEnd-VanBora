using VanBora.Application.DTOs.Viagens;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IRelatorioService
{
    Task<Result<RelatorioResponse>> GerarRelatorioAsync(Guid gerenteUsuarioId, Guid viagemId, CancellationToken cancellationToken = default);
}
