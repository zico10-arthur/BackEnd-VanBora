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

        var slugNormalizado = request.Slug.Trim().ToLowerInvariant();

        // Guard: slug já em uso
        if (await _usuarioRepo.GetBySlugAsync(slugNormalizado, cancellationToken) is not null)
            return Error.Conflict("SLUG_DUPLICADO", "Slug já cadastrado.");

        // Verifica CPF existente
        var existentePorCpf = await _usuarioService
            .BuscarPorCpfAsync(request.Cpf, cancellationToken)
            .ConfigureAwait(false);

        // Se CPF já existe, decide o que fazer baseado no Tipo
        if (existentePorCpf is not null)
            return await TratarCpfExistenteNoRegistroGerenteAsync(
                existentePorCpf, request, slugNormalizado, cancellationToken);

        // CPF não existe → cria novo gerente
        return await CriarNovoGerenteAsync(request, slugNormalizado, cancellationToken);
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

        // Guard: idempotente — usuário já é Passageiro com senha
        if (ExistenteEPassageiroComSenha(existentePorCpf))
            return MontarRespostaPassageiro(existentePorCpf!);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var resultado = await ResolverRegistroPassageiroAsync(
                existentePorCpf, request, cancellationToken);

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

    // ── Métodos auxiliares ─────────────────────────────────────────

    private async Task<Result<RegistrarGerenteResponse>> TratarCpfExistenteNoRegistroGerenteAsync(
        Usuario existente,
        RegistrarGerenteRequest request,
        string slugNormalizado,
        CancellationToken cancellationToken)
    {
        return existente.Tipo switch
        {
            TipoUsuario.Passageiro => await UpgradePassageiroParaGerenteAsync(
                existente, request, slugNormalizado, cancellationToken),

            TipoUsuario.Gerente => Error.Conflict(
                "CPF_JA_CADASTRADO", "CPF já cadastrado como Gerente."),

            _ => Error.Conflict("TIPO_INCOMPATIVEL",
                $"Usuário com CPF informado é do tipo {existente.Tipo} e não pode ser convertido para Gerente.")
        };
    }

    private async Task<Result<RegistrarGerenteResponse>> CriarNovoGerenteAsync(
        RegistrarGerenteRequest request,
        string slugNormalizado,
        CancellationToken cancellationToken)
    {
        // Guard: email duplicado
        if (await _usuarioService.BuscarPorEmailAsync(request.Email, cancellationToken) is not null)
            return Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var (gratuito, taxa) = await CalcularTaxaAsync(cancellationToken);

            var cpf = CPF.Criar(request.Cpf);
            if (cpf.IsFailure) return cpf.Error;

            var email = Email.Criar(request.Email);
            if (email.IsFailure) return email.Error;

            var telefone = CriarTelefone(request.Telefone);
            if (telefone is not null && telefone.IsFailure) return telefone.Error;

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            var usuario = Usuario.CriarGerente(
                request.Nome, cpf!.Value, email!.Value, senhaHash,
                telefone?.Value, slugNormalizado, taxa, gratuito, request.ChavePix);

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

    private async Task<Result<RegistrarGerenteResponse>> UpgradePassageiroParaGerenteAsync(
        Usuario passageiro,
        RegistrarGerenteRequest request,
        string slugNormalizado,
        CancellationToken cancellationToken)
    {
        // Guard: email já em uso por outro usuário
        var donoDoEmail = await _usuarioService.BuscarPorEmailAsync(request.Email, cancellationToken);
        if (donoDoEmail is not null && donoDoEmail.Id != passageiro.Id)
            return Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado por outro usuário.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var (gratuito, taxa) = await CalcularTaxaAsync(cancellationToken);

            var email = Email.Criar(request.Email);
            if (email.IsFailure) return email.Error;

            var telefone = CriarTelefone(request.Telefone);
            if (telefone is not null && telefone.IsFailure) return telefone.Error;

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            passageiro.AtualizarDados(request.Nome, email.Value, telefone?.Value);
            passageiro.DefinirSenha(senhaHash);
            passageiro.UpgradeParaGerente(slugNormalizado, taxa, gratuito, request.ChavePix);

            _usuarioRepo.Update(passageiro);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return CriarRespostaGerente(passageiro);
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

    private async Task<Result<RegistrarPassageiroResponse>> ResolverRegistroPassageiroAsync(
        Usuario? existentePorCpf,
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken)
    {
        var semSenha = string.IsNullOrWhiteSpace(existentePorCpf?.SenhaHash);

        return (existentePorCpf, semSenha) switch
        {
            (null, _) => await RegistrarNovoPassageiroAsync(request, cancellationToken),
            (_, true) => await CompletarCadastroContaPendenteAsync(
                existentePorCpf!, request, cancellationToken),
            (_, false) => MontarRespostaPassageiro(existentePorCpf!)
        };
    }

    // ── Helpers estáticos ──────────────────────────────────────────

    private static bool ExistenteEPassageiroComSenha(Usuario? usuario) =>
        usuario is not null
        && !string.IsNullOrWhiteSpace(usuario.SenhaHash)
        && usuario.Tipo == TipoUsuario.Passageiro;

    private static Result<Telefone>? CriarTelefone(string? telefoneStr) =>
        string.IsNullOrWhiteSpace(telefoneStr) ? null : Telefone.Criar(telefoneStr);

    private async Task<(bool Gratuito, decimal Taxa)> CalcularTaxaAsync(CancellationToken cancellationToken)
    {
        var qtdGerentes = await _usuarioRepo.CountByTipoAsync(TipoUsuario.Gerente, cancellationToken);
        var gratuito = qtdGerentes < 2;
        return (gratuito, gratuito ? 0m : 5.0m);
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

    // ── US18: Atualizar Usuario ─────────────────────────────────────

    public async Task<Result<AtualizarUsuarioResponse>> AtualizarUsuarioAsync(
        Guid usuarioId,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Error.NotFound("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        return await AtualizarDadosUsuarioAsync(usuario, request, cancellationToken);
    }

    private async Task<Result<AtualizarUsuarioResponse>> AtualizarDadosUsuarioAsync(
        Usuario usuario,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        // Valida email
        var email = Email.Criar(request.Email);
        if (email.IsFailure) return email.Error;

        // Verifica se o email ja esta em uso por outro usuario
        var donoDoEmail = await _usuarioRepo.GetByEmailAsync(email.Value, cancellationToken);
        if (donoDoEmail is not null && donoDoEmail.Id != usuario.Id)
            return Error.Conflict("EMAIL_JA_CADASTRADO", "Email ja cadastrado por outro usuario.");

        // Valida telefone
        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telResult = Telefone.Criar(request.Telefone);
            if (telResult.IsFailure) return telResult.Error;
            telefone = telResult.Value;
        }

        // Atualiza dados basicos (nome, email, telefone)
        usuario.AtualizarDados(request.Nome, email.Value, telefone);

        // Senha
        if (!string.IsNullOrWhiteSpace(request.Senha))
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            usuario.DefinirSenha(senhaHash);
        }

        // Slug (apenas Gerente)
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            if (usuario.Tipo != TipoUsuario.Gerente)
                return Error.Forbidden("ACAO_NAO_PERMITIDA", "Apenas Gerentes podem alterar o slug.");

            var slugNormalizado = request.Slug.Trim().ToLowerInvariant();
            if (await _usuarioRepo.GetBySlugAsync(slugNormalizado, cancellationToken) is not null)
                return Error.Conflict("SLUG_DUPLICADO", "Slug ja cadastrado.");

            usuario.AtualizarSlug(slugNormalizado);
        }

        // ChavePix (apenas Gerente)
        if (request.ChavePix is not null)
        {
            if (usuario.Tipo != TipoUsuario.Gerente)
                return Error.Forbidden("ACAO_NAO_PERMITIDA", "Apenas Gerentes podem alterar a chave PIX.");

            usuario.AtualizarChavePix(string.IsNullOrWhiteSpace(request.ChavePix) ? null : request.ChavePix);
        }

        // CNH (apenas Motorista)
        if (!string.IsNullOrWhiteSpace(request.NumeroCNH))
        {
            if (usuario.Tipo != TipoUsuario.Motorista)
                return Error.Forbidden("ACAO_NAO_PERMITIDA", "Apenas Motoristas podem registrar CNH.");

            var cnh = CNH.Criar(request.NumeroCNH);
            if (cnh.IsFailure) return cnh.Error;

            usuario.RegistrarCNH(cnh.Value);
        }

        _usuarioRepo.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MontarRespostaAtualizacao(usuario);
    }

    private static Result<AtualizarUsuarioResponse> MontarRespostaAtualizacao(Usuario usuario)
    {
        return Result<AtualizarUsuarioResponse>.Success(new AtualizarUsuarioResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email?.Valor ?? string.Empty,
            Telefone = usuario.Telefone?.ValorCompleto,
            Cpf = usuario.CPF.Valor,
            Tipo = usuario.Tipo.ToString(),
            Slug = usuario.Slug,
            ChavePix = usuario.ChavePix,
            NumeroCNH = usuario.CNH?.Valor
        });
    }
}
