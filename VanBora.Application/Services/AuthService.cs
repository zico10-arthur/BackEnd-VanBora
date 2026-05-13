using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
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

    public AuthService(
        IUsuarioRepository usuarioRepo,
        IPerfilRepository perfilRepo,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IValidator<RegistrarGerenteRequest> registrarGerenteValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _usuarioRepo = usuarioRepo;
        _perfilRepo = perfilRepo;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _registrarGerenteValidator = registrarGerenteValidator;
        _loginValidator = loginValidator;
    }

    public async Task<Result<RegistrarGerenteResponse>> RegistrarGerente(
        RegistrarGerenteRequest request,
        CancellationToken cancellationToken = default)
    {
        // ────────────────────────────────────────────────────────────
        // PASSO 1: Validar dados de entrada (FluentValidation)
        // ────────────────────────────────────────────────────────────
        var validationResult = await _registrarGerenteValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var erros = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Error.Validation("DADOS_INVALIDOS", erros);
        }

        // ────────────────────────────────────────────────────────────
        // PASSO 2: Criar Value Objects
        // ────────────────────────────────────────────────────────────
        var cpfResult = CPF.Criar(request.Cpf);
        if (cpfResult.IsFailure)
            return cpfResult.Error;

        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telefoneResult = Telefone.Criar(request.Telefone);
            if (telefoneResult.IsFailure)
                return telefoneResult.Error;

            telefone = telefoneResult.Value;
        }

        // ────────────────────────────────────────────────────────────
        // PASSO 3: Verificar slug único
        // ────────────────────────────────────────────────────────────
        var slugNormalized = request.Slug.Trim().ToLowerInvariant();
        var perfilExistenteSlug = await _perfilRepo.GetBySlugAsync(slugNormalized, cancellationToken);
        if (perfilExistenteSlug is not null)
            return Error.Conflict("SLUG_DUPLICADO", "Slug já cadastrado.");

        // ────────────────────────────────────────────────────────────
        // PASSO 4: Buscar ou criar Usuario por CPF (RN10)
        // ────────────────────────────────────────────────────────────
        Usuario usuario;
        var usuarioExistente = await _usuarioRepo.GetByCpfAsync(cpfResult.Value, cancellationToken);

        if (usuarioExistente is null)
        {
            // ── CPF não existe → criar novo Usuario ──
            var emailExistente = await _usuarioRepo.GetByEmailAsync(emailResult.Value, cancellationToken);
            if (emailExistente is not null)
                return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            usuario = new Usuario(
                request.Nome,
                cpfResult.Value,
                emailResult.Value,
                senhaHash,
                telefone);

            await _usuarioRepo.AddAsync(usuario, cancellationToken);

            // Criar Perfil Passageiro automático
            var perfilPassageiro = Perfil.CriarPassageiro(usuario.Id);
            usuario.AdicionarPerfil(perfilPassageiro);
            await _perfilRepo.AddAsync(perfilPassageiro, cancellationToken);
        }
        else
        {
            // ── CPF já existe → reutilizar Usuario ──
            usuario = usuarioExistente;

            // Verificar se já possui Perfil Gerente
            if (usuario.Perfis.Any(p => p.Tipo == Domain.Enums.TipoPerfil.Gerente))
                return Error.Conflict("GERENTE_EXISTENTE", "Usuário já possui perfil de gerente.");

            if (usuario.Email is null)
            {
                // Usuario existente sem email (ex-Motorista):
                // verificar se o email não pertence a outro usuario
                var emailDeOutroUsuario = await _usuarioRepo.GetByEmailAsync(emailResult.Value, cancellationToken);

                if (emailDeOutroUsuario is not null && emailDeOutroUsuario.Id != usuario.Id)
                    return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

                // Atualizar dados do usuario
                var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
                usuario.AtualizarDados(request.Nome, emailResult.Value, telefone);
                usuario.DefinirSenha(senhaHash);
            }
            else
            {
                // Usuario já possui email → apenas adiciona o perfil Gerente
                // A senha informada no request é IGNORADA
                if (telefone is not null)
                {
                    usuario.AtualizarDados(usuario.Nome, usuario.Email, telefone);
                }
            }
        }

        // ────────────────────────────────────────────────────────────
        // PASSO 5: Definir taxa (Fluxo 0800 — RN03/RN16)
        // ────────────────────────────────────────────────────────────
        var totalGerentes = await _perfilRepo.GetByTipoAsync(Domain.Enums.TipoPerfil.Gerente, cancellationToken);
        var gratuito = totalGerentes.Count < 2;
        var taxa = gratuito ? 0m : 5.0m;

        // ────────────────────────────────────────────────────────────
        // PASSO 6: Criar Perfil Gerente
        // ────────────────────────────────────────────────────────────
        var perfilGerente = Perfil.CriarGerente(usuario.Id, slugNormalized, taxa, gratuito);
        usuario.AdicionarPerfil(perfilGerente);
        await _perfilRepo.AddAsync(perfilGerente, cancellationToken);

        // ────────────────────────────────────────────────────────────
        // PASSO 7: Persistir (Unit of Work)
        // ────────────────────────────────────────────────────────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ────────────────────────────────────────────────────────────
        // PASSO 8: Gerar Token JWT
        // ────────────────────────────────────────────────────────────
        var perfis = usuario.Perfis.Select(p => p.Tipo.ToString()).ToList();
        var emailParaToken = usuario.Email?.Valor ?? emailResult.Value.Valor;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, perfis);

        // ────────────────────────────────────────────────────────────
        // PASSO 9: Retornar resposta
        // ────────────────────────────────────────────────────────────
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

    // ────────────────────────────────────────────────────────────
    // Login — US02 (Gerente) e US04 (Passageiro)
    // ────────────────────────────────────────────────────────────
    public async Task<Result<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // ── PASSO 1: Validar dados de entrada (FluentValidation) ──
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            // Mensagem genérica: não revela se o erro é no email ou na senha
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");
        }

        // ── PASSO 2: Criar Value Object Email ──
        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure)
            return Error.Validation("EMAIL_INVALIDO", "Formato de email inválido");

        // ── PASSO 3: Buscar usuário por email ──
        var usuario = await _usuarioRepo.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (usuario is null)
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        // ── PASSO 4: Verificar se a conta está ativa ──
        if (!usuario.Ativo)
            return Error.Forbidden("CONTA_DESATIVADA", "Conta desativada");

        // ── PASSO 5: Verificar se o usuário possui senha (ativação pendente) ──
        if (usuario.SenhaHash is null)
            return Error.Unauthorized("CONTA_SEM_SENHA", "Conta ainda não ativada. Registre-se como passageiro primeiro");

        // ── PASSO 6: Verificar senha com BCrypt ──
        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Error.Unauthorized("CREDENCIAIS_INVALIDAS", "Email ou senha inválidos");

        // ── PASSO 7: Buscar perfis ativos do usuário ──
        var perfis = await _perfilRepo.GetByUsuarioIdAsync(usuario.Id, cancellationToken);
        var perfisAtivos = perfis
            .Where(p => p.Ativo)
            .Select(p => p.Tipo.ToString())
            .ToList();

        // ── PASSO 8: Gerar token JWT ──
        var emailParaToken = usuario.Email?.Valor ?? string.Empty;
        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, emailParaToken, perfisAtivos);

        // ── PASSO 9: Retornar resposta ──
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
