using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IMotoristaService
{
    Task<Result<RegistrarMotoristaResponse>> RegistrarMotorista(Guid gerenteId, RegistrarMotoristaRequest request, CancellationToken ct = default);
    Task<Result<List<RegistrarMotoristaResponse>>> ListarMotoristas(Guid gerenteId, CancellationToken ct = default);
    Task<Result<RegistrarMotoristaResponse>> AtualizarMotorista(Guid gerenteId, Guid motoristaId, RegistrarMotoristaRequest request, CancellationToken ct = default);
    Task<Result<bool>> RemoverMotorista(Guid gerenteId, Guid motoristaId, CancellationToken ct = default);
}
