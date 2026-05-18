using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILoginService _loginService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IValidator<RegistrarGerenteRequest> _gerenteValidator;
    private readonly IValidator<RegistrarPassageiroRequest> _passageiroValidator;

    public AuthService(
        IUsuarioService usuarioService,
        ILoginService loginService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IUsuarioRepository usuarioRepo,
        IValidator<RegistrarGerenteRequest> gerenteValidator,
        IValidator<RegistrarPassageiroRequest> passageiroValidator)
    {
        _usuarioService = usuarioService;
        _loginService = loginService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _usuarioRepo = usuarioRepo;
        _gerenteValidator = gerenteValidator;
        _passageiroValidator = passageiroValidator;
    }

    public async Task<Result<RegistrarGerenteResponse>> RegistrarGerente(
        RegistrarGerenteRequest request,
        CancellationToken cancellationToken = default)
    {
        var erroValidacao = await ValidarRequestAsync(request, _gerenteValidator, cancellationToken);
        if (erroValidacao is not null) return erroValidacao;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Verifica se CPF já está em uso
            var existentePorCpf = await _usuarioService
                .BuscarPorCpfAsync(request.Cpf, cancellationToken)
                .ConfigureAwait(false);

            if (existentePorCpf is not null)
                return Error.Conflict("CPF_JA_CADASTRADO", "CPF já cadastrado.");

            // Verifica slug único
            var slugNormalizado = request.Slug.Trim().ToLowerInvariant();
            if (await _usuarioRepo.GetBySlugAsync(slugNormalizado, cancellationToken) is not null)
                return Error.Conflict("SLUG_DUPLICADO", "Slug já cadastrado.");

            // Calcula taxa
            var qtdGerentes = await _usuarioRepo.CountByTipoAsync(TipoUsuario.Gerente, cancellationToken);
            var gratuito = qtdGerentes < 2;
            var taxa = gratuito ? 0m : 5.0m;

            // Cria Value Objects
            var cpf = CPF.Criar(request.Cpf);
            if (cpf.IsFailure) return cpf.Error;

            var email = Email.Criar(request.Email);
            if (email.IsFailure) return email.Error;

            Telefone? telefone = null;
            if (!string.IsNullOrWhiteSpace(request.Telefone))
            {
                var telResult = Telefone.Criar(request.Telefone);
                if (telResult.IsFailure) return telResult.Error;
                telefone = telResult.Value;
            }

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            // Cria o Gerente diretamente (Tipo = Gerente)
            var usuario = Usuario.CriarGerente(
                request.Nome, cpf.Value, email.Value, senhaHash,
                telefone, slugNormalizado, taxa, gratuito, request.ChavePix);

            await _usuarioRepo.AddAsync(usuario, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return CriarRespostaGerente(usuario);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginResult = await _loginService.LoginAsync(request, cancellationToken);
        if (loginResult.IsFailure) return loginResult.Error;

        var (usuario, tipos) = loginResult.Value;
        return CriarRespostaLogin(usuario, tipos);
    }

    public async Task<Result<RegistrarPassageiroResponse>> RegistrarPassageiroAsync(
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken = default)
    {
        var erroValidacao = await ValidarRequestAsync(request, _passageiroValidator, cancellationToken);
        if (erroValidacao is not null) return erroValidacao;

        var existentePorCpf = await _usuarioService
            .BuscarPorCpfAsync(request.Cpf, cancellationToken)
            .ConfigureAwait(false);

        // Caso idempotente: usuário já existe como Passageiro com senha definida
        if (existentePorCpf is not null
            && !string.IsNullOrWhiteSpace(existentePorCpf.SenhaHash)
            && existentePorCpf.Tipo == TipoUsuario.Passageiro)
        {
            return MontarRespostaPassageiro(existentePorCpf);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Result<RegistrarPassageiroResponse> resultado;

            if (existentePorCpf is null)
                resultado = await RegistrarNovoPassageiroAsync(request, cancellationToken);
            else if (string.IsNullOrWhiteSpace(existentePorCpf.SenhaHash))
                resultado = await CompletarCadastroContaPendenteAsync(
                    existentePorCpf, request, cancellationToken);
            else
                resultado = MontarRespostaPassageiro(existentePorCpf);

            if (resultado.IsFailure)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return resultado;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return resultado;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
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

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuarioResult.Value);
    }

    private async Task<Result<RegistrarPassageiroResponse>> CompletarCadastroContaPendenteAsync(
        Usuario usuario, RegistrarPassageiroRequest request,
        CancellationToken cancellationToken)
    {
        var atualizado = await _usuarioService.AtualizarContaPendenteAsync(
            usuario, request.Nome, request.Email, request.Telefone, request.Senha, cancellationToken);
        if (atualizado.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(atualizado.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private Result<RegistrarGerenteResponse> CriarRespostaGerente(Usuario usuario)
    {
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var tipos = new List<string> { usuario.Tipo.ToString() };
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, tipos);

        return Result<RegistrarGerenteResponse>.Success(new RegistrarGerenteResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email?.Valor,
            Telefone = usuario.Telefone?.ValorCompleto,
            Cpf = usuario.CPF.Valor,
            Gerente = new GerenteResponse
            {
                UsuarioId = usuario.Id,
                Slug = usuario.Slug!,
                TaxaPlataforma = usuario.TaxaPlataforma!.Value,
                Gratuito = usuario.Gratuito!.Value,
                Ativo = usuario.Ativo
            },
            Tipo = usuario.Tipo.ToString(),
            Token = token
        });
    }

    private Result<LoginResponse> CriarRespostaLogin(Usuario usuario, List<string> tipos)
    {
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, tipos);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = emailParaToken,
            Perfis = tipos,
            Token = token
        });
    }

    private Result<RegistrarPassageiroResponse> MontarRespostaPassageiro(Usuario usuario)
    {
        var tipos = new List<string> { usuario.Tipo.ToString() };
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, tipos);

        return Result<RegistrarPassageiroResponse>.Success(
            new RegistrarPassageiroResponse(usuario.Id, token));
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
