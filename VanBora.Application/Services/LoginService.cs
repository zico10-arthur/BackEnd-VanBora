using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class LoginService : ILoginService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPerfilService _perfilService;
    private readonly IValidator<LoginRequest> _loginValidator;

    public LoginService(
        IUsuarioRepository usuarioRepo,
        IPerfilService perfilService,
        IValidator<LoginRequest> loginValidator)
    {
        _usuarioRepo = usuarioRepo;
        _perfilService = perfilService;
        _loginValidator = loginValidator;
    }

    public async Task<Result<(Usuario usuario, List<string> perfisAtivos)>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var validacao = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure)
            return Error.Validation("EMAIL_INVALIDO", "Formato de email inválido");
        var email = emailResult.Value;

        var usuario = await _usuarioRepo.GetByEmailAsync(email, cancellationToken);
        if (usuario is null)
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        var erroEstado = VerificarEstadoConta(usuario);
        if (erroEstado is not null)
            return erroEstado;

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        var perfisAtivos = await _perfilService.BuscarPerfisAtivosAsync(usuario.Id, cancellationToken);

        return Result<(Usuario, List<string>)>.Success((usuario, perfisAtivos));
    }

    private static Error? VerificarEstadoConta(Usuario usuario)
    {
        if (!usuario.Ativo)
            return Error.Forbidden("CONTA_DESATIVADA", "Conta desativada");

        if (usuario.SenhaHash is null)
            return Error.Unauthorized("CONTA_SEM_SENHA", "Conta ainda não ativada. Registre-se como passageiro primeiro");

        return null;
    }
}
