using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;

namespace VanBora.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioService _usuarioService;
    private readonly IPerfilService _perfilService;
    private readonly ILoginService _loginService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegistrarGerenteRequest> _gerenteValidator;
    private readonly IValidator<RegistrarPassageiroRequest> _passageiroValidator;

    public AuthService(
        IUsuarioService usuarioService,
        IPerfilService perfilService,
        ILoginService loginService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IValidator<RegistrarGerenteRequest> gerenteValidator,
        IValidator<RegistrarPassageiroRequest> passageiroValidator)
    {
        _usuarioService = usuarioService;
        _perfilService = perfilService;
        _loginService = loginService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _gerenteValidator = gerenteValidator;
        _passageiroValidator = passageiroValidator;
    }

    public async Task<Result<RegistrarGerenteResponse>> RegistrarGerente(
        RegistrarGerenteRequest request,
        CancellationToken cancellationToken = default)
    {
        var erroValidacao = await ValidarRequestAsync(request, _gerenteValidator, cancellationToken);
        if (erroValidacao is not null) return erroValidacao;

        var usuarioResult = await _usuarioService.ObterOuCriarAsync(
            request.Nome, request.Cpf, request.Email, request.Telefone, request.Senha, cancellationToken);
        if (usuarioResult.IsFailure) return usuarioResult.Error;
        var usuario = usuarioResult.Value;

        var perfilResult = await _perfilService.CriarPerfilGerenteAsync(
            usuario, request.Slug, cancellationToken);
        if (perfilResult.IsFailure) return perfilResult.Error;
        var perfilGerente = perfilResult.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CriarRespostaGerente(usuario, perfilGerente);
    }

    public async Task<Result<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginResult = await _loginService.LoginAsync(request, cancellationToken);
        if (loginResult.IsFailure) return loginResult.Error;

        var (usuario, perfisAtivos) = loginResult.Value;
        return CriarRespostaLogin(usuario, perfisAtivos);
    }

    public async Task<Result<RegistrarPassageiroResponse>> RegistrarPassageiroAsync(
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validacao = await _passageiroValidator
            .ValidateAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (!validacao.IsValid)
        {
            var mensagem = string.Join(" ", validacao.Errors.Select(e => e.ErrorMessage));
            return Result<RegistrarPassageiroResponse>.Failure(
                Error.Validation("VALIDACAO_DTO", mensagem));
        }

        var existentePorCpf = await _usuarioService
            .BuscarPorCpfAsync(request.Cpf, cancellationToken)
            .ConfigureAwait(false);

        if (existentePorCpf is null)
            return await RegistrarNovoPassageiroAsync(request, cancellationToken);

        if (string.IsNullOrWhiteSpace(existentePorCpf.SenhaHash))
            return await CompletarCadastroContaPendenteAsync(
                    existentePorCpf, request, cancellationToken);

        if (existentePorCpf.Perfis.Any(p => p.Tipo == TipoPerfil.Passageiro))
            return MontarRespostaPassageiro(existentePorCpf);

        return await AdicionarPerfilPassageiroAsync(existentePorCpf, cancellationToken);
    }

    private async Task<Result<RegistrarPassageiroResponse>> RegistrarNovoPassageiroAsync(
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken)
    {
        if (await _usuarioService.BuscarPorEmailAsync(request.Email, cancellationToken)
                .ConfigureAwait(false) is not null)
            return Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado.");

        var usuarioResult = await _usuarioService.ObterOuCriarAsync(
            request.Nome, request.Cpf, request.Email, request.Telefone, request.Senha, cancellationToken);
        if (usuarioResult.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(usuarioResult.Error);

        var usuario = usuarioResult.Value;

        var perfilPassageiro = _perfilService.CriarPerfilPassageiro(usuario.Id);
        usuario.AdicionarPerfil(perfilPassageiro);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private async Task<Result<RegistrarPassageiroResponse>> CompletarCadastroContaPendenteAsync(
        Usuario usuario, RegistrarPassageiroRequest request,
        CancellationToken cancellationToken)
    {
        var atualizado = await _usuarioService.AtualizarContaPendenteAsync(
            usuario, request.Nome, request.Email, request.Telefone, request.Senha, cancellationToken);
        if (atualizado.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(atualizado.Error);

        if (!usuario.Perfis.Any(p => p.Tipo == TipoPerfil.Passageiro))
        {
            var perfilPassageiro = _perfilService.CriarPerfilPassageiro(usuario.Id);
            usuario.AdicionarPerfil(perfilPassageiro);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private async Task<Result<RegistrarPassageiroResponse>> AdicionarPerfilPassageiroAsync(
        Usuario usuario, CancellationToken cancellationToken)
    {
        var perfilPassageiro = _perfilService.CriarPerfilPassageiro(usuario.Id);
        usuario.AdicionarPerfil(perfilPassageiro);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private Result<RegistrarGerenteResponse> CriarRespostaGerente(Usuario usuario, Perfil perfilGerente)
    {
        var perfis = usuario.Perfis.Select(p => p.Tipo.ToString()).ToList();
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, perfis);

        return Result<RegistrarGerenteResponse>.Success(new RegistrarGerenteResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email?.Valor,
            Telefone = usuario.Telefone?.ValorCompleto,
            Cpf = usuario.CPF.Valor,
            Gerente = new GerenteResponse
            {
                PerfilId = perfilGerente.Id,
                Slug = perfilGerente.Slug!,
                TaxaPlataforma = perfilGerente.TaxaPlataforma!.Value,
                Gratuito = perfilGerente.Gratuito!.Value,
                Ativo = perfilGerente.Ativo
            },
            Perfis = perfis,
            Token = token
        });
    }

    private Result<LoginResponse> CriarRespostaLogin(Usuario usuario, List<string> perfisAtivos)
    {
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, perfisAtivos);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = emailParaToken,
            Perfis = perfisAtivos,
            Token = token
        });
    }

    private Result<RegistrarPassageiroResponse> MontarRespostaPassageiro(Usuario usuario)
    {
        var perfilPassageiroId = usuario.Perfis
            .First(p => p.Tipo == TipoPerfil.Passageiro)
            .Id;

        var perfisAtivos = usuario.Perfis
            .Where(p => p.Ativo)
            .Select(p => p.Tipo.ToString())
            .ToList();
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, perfisAtivos);

        return Result<RegistrarPassageiroResponse>.Success(
            new RegistrarPassageiroResponse(usuario.Id, perfilPassageiroId, token));
    }

    private static async Task<Error?> ValidarRequestAsync<T>(
        T request, IValidator<T> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid) return null;

        var primeiroErro = result.Errors.First();
        return Error.Validation(primeiroErro.ErrorCode, primeiroErro.ErrorMessage);
    }
}
