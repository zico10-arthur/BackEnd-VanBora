using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Common;
namespace VanBora.Application.Interfaces;

public interface IMotoristaService
{
    Task<Result<RegistrarMotoristaResponse>> RegistrarMotorista(Guid gerenteid, RegistrarMotoristaRequest registrarmotorista, CancellationToken ct = default);
}
