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
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPerfilRepository _perfilRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegistrarGerenteRequest> _registrarGerenteValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegistrarPassageiroRequest> _registrarPassageiroValidator;

    public AuthService(
        IUsuarioRepository usuarioRepo,
        IPerfilRepository perfilRepo,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IValidator<RegistrarGerenteRequest> registrarGerenteValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegistrarPassageiroRequest> registrarPassageiroValidator)
    {
        _usuarioRepo = usuarioRepo;
        _perfilRepo = perfilRepo;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _registrarGerenteValidator = registrarGerenteValidator;
        _loginValidator = loginValidator;
        _registrarPassageiroValidator = registrarPassageiroValidator;
    }

    // ════════════════════════════════════════════════════════════════
    //  ORQUESTRAÇÃO — Registrar Gerente
    // ════════════════════════════════════════════════════════════════
    public async Task<Result<RegistrarGerenteResponse>> RegistrarGerente(
        RegistrarGerenteRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request (FluentValidation)
        var erroValidacao = await ValidarRegistroAsync(request, cancellationToken);
        if (erroValidacao is not null) return erroValidacao;

        // 2. Extrair e validar Value Objects do request
        var valueObjects = ExtrairValueObjects(request);
        if (valueObjects.IsFailure) return valueObjects.Error;
        var (cpf, email, telefone) = valueObjects.Value;

        // 3. Verificar slug único
        var slug = NormalizarSlug(request.Slug);
        if (await SlugJaEmUsoAsync(slug, cancellationToken))
            return Error.Conflict("SLUG_DUPLICADO", "Slug já cadastrado.");

        // 4. Obter usuário (criar novo ou reutilizar existente via RN10)
        var usuarioResult = await ObterUsuarioAsync(cpf, email, request.Nome, request.Senha, telefone, cancellationToken);
        if (usuarioResult.IsFailure) return usuarioResult.Error;
        var usuario = usuarioResult.Value;

        // 5. Calcular taxa (Fluxo 0800 — gratuito para < 2 gerentes)
        var (gratuito, taxa) = await CalcularTaxaAsync(cancellationToken);

        // 6. Criar Perfil Gerente + persistir transação
        var perfilGerente = CriarPerfilGerente(usuario, slug, gratuito, taxa);
        await _perfilRepo.AddAsync(perfilGerente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Montar resposta com token JWT
        return CriarResposta(usuario, perfilGerente, email);
    }

    // ════════════════════════════════════════════════════════════════
    //  ORQUESTRAÇÃO — Login
    // ════════════════════════════════════════════════════════════════
    public async Task<Result<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        var (valido, erro) = await ValidarLoginAsync(request, cancellationToken);
        if (!valido) return erro!;

        // 2. Criar Value Object Email
        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure)
            return Error.Validation("EMAIL_INVALIDO", "Formato de email inválido");
        var email = emailResult.Value;

        // 3. Buscar usuário por email
        var usuario = await _usuarioRepo.GetByEmailAsync(email, cancellationToken);
        if (usuario is null)
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        // 4. Verificar estado da conta
        var erroEstado = VerificarEstadoConta(usuario);
        if (erroEstado is not null) return erroEstado;

        // 5. Verificar senha
        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        // 6. Buscar perfis ativos
        var perfisAtivos = await BuscarPerfisAtivosAsync(usuario.Id, cancellationToken);

        // 7. Gerar token + resposta
        return CriarRespostaLogin(usuario, perfisAtivos);
    }

    public async Task<Result<RegistrarPassageiroResponse>> RegistrarPassageiroAsync(
        RegistrarPassageiroRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validacao = await _registrarPassageiroValidator
            .ValidateAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (!validacao.IsValid)
        {
            var mensagem = string.Join(" ", validacao.Errors.Select(e => e.ErrorMessage));
            return Result<RegistrarPassageiroResponse>.Failure(
                Error.Validation("VALIDACAO_DTO", mensagem));
        }

        var cpfResultado = CPF.Criar(request.Cpf);
        if (cpfResultado.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(cpfResultado.Error);

        var emailResultado = Email.Criar(request.Email);
        if (emailResultado.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(emailResultado.Error);

        var telefoneResultado = Telefone.Criar(request.Telefone);
        if (telefoneResultado.IsFailure)
            return Result<RegistrarPassageiroResponse>.Failure(telefoneResultado.Error);

        var cpf = cpfResultado.Value;
        var email = emailResultado.Value;
        var telefone = telefoneResultado.Value;
        var nome = request.Nome.Trim();

        var existentePorCpf = await _usuarioRepo
            .GetByCpfAsync(cpf, cancellationToken)
            .ConfigureAwait(false);

        if (existentePorCpf is null)
            return await RegistrarNovoPassageiroAsync(nome, cpf, email, telefone, request.Senha, cancellationToken)
                .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(existentePorCpf.SenhaHash))
        {
            return Result<RegistrarPassageiroResponse>.Failure(
                Error.Conflict("CPF_JA_CADASTRADO", "CPF já cadastrado."));
        }

        return await CompletarCadastroContaPendenteAsync(
                existentePorCpf, nome, email, telefone, request.Senha, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Result<RegistrarPassageiroResponse>> RegistrarNovoPassageiroAsync(
        string nome,
        CPF cpf,
        Email email,
        Telefone telefone,
        string senhaPlana,
        CancellationToken cancellationToken)
    {
        if (await _usuarioRepo.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false) is not null)
        {
            return Result<RegistrarPassageiroResponse>.Failure(
                Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado."));
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaPlana);
        var usuario = new Usuario(nome, cpf, email, senhaHash, telefone);
        var perfilPassageiro = Perfil.CriarPassageiro(usuario.Id);
        usuario.AdicionarPerfil(perfilPassageiro);

        await _usuarioRepo.AddAsync(usuario, cancellationToken).ConfigureAwait(false);
        await _perfilRepo.AddAsync(perfilPassageiro, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private async Task<Result<RegistrarPassageiroResponse>> CompletarCadastroContaPendenteAsync(
        Usuario usuario,
        string nome,
        Email email,
        Telefone telefone,
        string senhaPlana,
        CancellationToken cancellationToken)
    {
        var donoDoEmail = await _usuarioRepo.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (donoDoEmail is not null && donoDoEmail.Id != usuario.Id)
        {
            return Result<RegistrarPassageiroResponse>.Failure(
                Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado."));
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaPlana);
        usuario.AtualizarDados(nome, email, telefone);
        usuario.DefinirSenha(senhaHash);
        usuario.Ativar();

        if (!usuario.Perfis.Any(p => p.Tipo == TipoPerfil.Passageiro))
        {
            var perfilPassageiro = Perfil.CriarPassageiro(usuario.Id);
            usuario.AdicionarPerfil(perfilPassageiro);
            await _perfilRepo.AddAsync(perfilPassageiro, cancellationToken).ConfigureAwait(false);
        }

        _usuarioRepo.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MontarRespostaPassageiro(usuario);
    }

    private Result<RegistrarPassageiroResponse> MontarRespostaPassageiro(Usuario usuario)
    {
        var perfilPassageiroId = usuario.Perfis
            .First(p => p.Tipo == TipoPerfil.Passageiro)
            .Id;

        var token = _tokenService.GerarToken(usuario);

        return Result<RegistrarPassageiroResponse>.Success(
            new RegistrarPassageiroResponse(usuario.Id, perfilPassageiroId, token));
    }

    // ════════════════════════════════════════════════════════════════
    //  MÉTODOS PRIVADOS — Registrar Gerente
    // ════════════════════════════════════════════════════════════════

    private async Task<Error?> ValidarRegistroAsync(RegistrarGerenteRequest request, CancellationToken ct)
    {
        var result = await _registrarGerenteValidator.ValidateAsync(request, ct);
        if (result.IsValid) return null;

        var erros = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
        return Error.Validation("DADOS_INVALIDOS", erros);
    }

    private static Result<(CPF cpf, Email email, Telefone? telefone)> ExtrairValueObjects(RegistrarGerenteRequest request)
    {
        var cpfResult = CPF.Criar(request.Cpf);
        if (cpfResult.IsFailure) return cpfResult.Error;

        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure) return emailResult.Error;

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telefoneResult = Telefone.Criar(request.Telefone);
            if (telefoneResult.IsFailure) return telefoneResult.Error;
            telefone = telefoneResult.Value;
        }

        return Result<(CPF, Email, Telefone?)>.Success((cpfResult.Value, emailResult.Value, telefone));
    }

    private static string NormalizarSlug(string slug)
        => slug.Trim().ToLowerInvariant();

    private async Task<bool> SlugJaEmUsoAsync(string slug, CancellationToken ct)
    {
        var existente = await _perfilRepo.GetBySlugAsync(slug, ct);
        return existente is not null;
    }

    private async Task<Result<Usuario>> ObterUsuarioAsync(
        CPF cpf, Email email, string nome, string senha, Telefone? telefone,
        CancellationToken ct)
    {
        var existente = await _usuarioRepo.GetByCpfAsync(cpf, ct);

        if (existente is null)
            return await CriarUsuarioComPerfilPassageiroAsync(nome, cpf, email, senha, telefone, ct);

        return await ValidarEAtualizarUsuarioExistenteAsync(existente, email, nome, senha, telefone, ct);
    }

    private async Task<Result<Usuario>> CriarUsuarioComPerfilPassageiroAsync(
        string nome, CPF cpf, Email email, string senha, Telefone? telefone,
        CancellationToken ct)
    {
        var emailExistente = await _usuarioRepo.GetByEmailAsync(email, ct);
        if (emailExistente is not null)
            return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = new Usuario(nome, cpf, email, senhaHash, telefone);

        await _usuarioRepo.AddAsync(usuario, ct);

        var perfilPassageiro = Perfil.CriarPassageiro(usuario.Id);
        usuario.AdicionarPerfil(perfilPassageiro);
        await _perfilRepo.AddAsync(perfilPassageiro, ct);

        return Result<Usuario>.Success(usuario);
    }

    private async Task<Result<Usuario>> ValidarEAtualizarUsuarioExistenteAsync(
        Usuario usuario, Email email, string nome, string senha, Telefone? telefone,
        CancellationToken ct)
    {
        if (usuario.Perfis.Any(p => p.Tipo == TipoPerfil.Gerente))
            return Error.Conflict("GERENTE_EXISTENTE", "Usuário já possui perfil de gerente.");

        // Usuário sem email (ex-Motorista): precisa configurar dados completos
        if (usuario.Email is null)
        {
            var emailDeOutro = await _usuarioRepo.GetByEmailAsync(email, ct);
            if (emailDeOutro is not null && emailDeOutro.Id != usuario.Id)
                return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
            usuario.AtualizarDados(nome, email, telefone);
            usuario.DefinirSenha(senhaHash);

            return Result<Usuario>.Success(usuario);
        }

        // Usuário já completo: só atualiza telefone (senha do request é ignorada)
        if (telefone is not null)
            usuario.AtualizarDados(usuario.Nome, usuario.Email, telefone);

        return Result<Usuario>.Success(usuario);
    }

    private async Task<(bool gratuito, decimal taxa)> CalcularTaxaAsync(CancellationToken ct)
    {
        var totalGerentes = await _perfilRepo.GetByTipoAsync(TipoPerfil.Gerente, ct);
        var gratuito = totalGerentes.Count < 2;
        var taxa = gratuito ? 0m : 5.0m;
        return (gratuito, taxa);
    }

    private static Perfil CriarPerfilGerente(Usuario usuario, string slug, bool gratuito, decimal taxa)
    {
        var perfil = Perfil.CriarGerente(usuario.Id, slug, taxa, gratuito);
        usuario.AdicionarPerfil(perfil);
        return perfil;
    }

    private Result<RegistrarGerenteResponse> CriarResposta(Usuario usuario, Perfil perfilGerente, Email email)
    {
        var perfis = usuario.Perfis.Select(p => p.Tipo.ToString()).ToList();
        var emailParaToken = usuario.Email?.Valor ?? email.Valor;
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

    // ════════════════════════════════════════════════════════════════
    //  MÉTODOS PRIVADOS — Login
    // ════════════════════════════════════════════════════════════════

    private async Task<(bool valido, Error? erro)> ValidarLoginAsync(LoginRequest request, CancellationToken ct)
    {
        var result = await _loginValidator.ValidateAsync(request, ct);
        if (result.IsValid) return (true, null);

        return (false, Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos"));
    }

    private static Error? VerificarEstadoConta(Usuario usuario)
    {
        if (!usuario.Ativo)
            return Error.Forbidden("CONTA_DESATIVADA", "Conta desativada");

        if (usuario.SenhaHash is null)
            return Error.Unauthorized("CONTA_SEM_SENHA", "Conta ainda não ativada. Registre-se como passageiro primeiro");

        return null;
    }

    private async Task<List<string>> BuscarPerfisAtivosAsync(Guid usuarioId, CancellationToken ct)
    {
        var perfis = await _perfilRepo.GetByUsuarioIdAsync(usuarioId, ct);
        return perfis
            .Where(p => p.Ativo)
            .Select(p => p.Tipo.ToString())
            .ToList();
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
}
