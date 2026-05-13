using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IAuthService
{
    Task<Result<RegistrarGerenteResponse>> RegistrarGerente(
        RegistrarGerenteRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
