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

    Task<Result<RegistrarPassageiroResponse>> RegistrarPassageiroAsync(
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AtualizarUsuarioResponse>> ObterUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<Result<AtualizarUsuarioResponse>> AtualizarUsuarioAsync(
        Guid usuarioId,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<string>> AlterarSenhaAsync(
        Guid usuarioId,
        AlterarSenhaRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AtualizarUsuarioResponse>> AtualizarSlugAsync(
        Guid usuarioId,
        AtualizarSlugRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SolicitarExclusaoResponse>> SolicitarExclusaoAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<Result<ConfirmarExclusaoResponse>> ConfirmarExclusaoAsync(
        Guid usuarioId,
        ConfirmarExclusaoRequest request,
        CancellationToken cancellationToken = default);
}
