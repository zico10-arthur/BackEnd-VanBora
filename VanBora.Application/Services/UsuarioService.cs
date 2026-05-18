using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;

    public UsuarioService(IUsuarioRepository usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    public async Task<Usuario?> BuscarPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfVo = CPF.Criar(cpf);
        if (cpfVo.IsFailure) return null;
        return await _usuarioRepo.GetByCpfAsync(cpfVo.Value, cancellationToken);
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailVo = Email.Criar(email);
        if (emailVo.IsFailure) return null;
        return await _usuarioRepo.GetByEmailAsync(emailVo.Value, cancellationToken);
    }

    public async Task<Result<Usuario>> ObterOuCriarAsync(
        string nome, string cpfStr, string emailStr, string? telefoneStr, string senha,
        CancellationToken cancellationToken = default)
    {
        var cpf = CriarCpf(cpfStr);
        if (cpf.IsFailure) return cpf.Error;

        var email = CriarEmail(emailStr);
        if (email.IsFailure) return email.Error;

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(telefoneStr))
        {
            var telResult = CriarTelefone(telefoneStr);
            if (telResult.IsFailure) return telResult.Error;
            telefone = telResult.Value;
        }

        var existente = await _usuarioRepo.GetByCpfAsync(cpf.Value, cancellationToken);

        if (existente is null)
            return await CriarNovoPassageiroAsync(nome, cpf.Value, email.Value, telefone, senha, cancellationToken);

        return await ValidarEAtualizarExistenteAsync(existente, nome, email.Value, senha, telefone, cancellationToken);
    }

    public async Task<Result<Usuario>> AtualizarContaPendenteAsync(
        Usuario usuario, string nome, string emailStr, string? telefoneStr, string senha,
        CancellationToken cancellationToken = default)
    {
        var email = CriarEmail(emailStr);
        if (email.IsFailure) return email.Error;

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(telefoneStr))
        {
            var telResult = CriarTelefone(telefoneStr);
            if (telResult.IsFailure) return telResult.Error;
            telefone = telResult.Value;
        }

        var emailDisponivel = await EmailDisponivelAsync(email.Value, usuario.Id, cancellationToken);
        if (!emailDisponivel)
            return Error.Conflict("EMAIL_JA_CADASTRADO", "Email já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        usuario.AtualizarDados(nome, email.Value, telefone);
        usuario.DefinirSenha(senhaHash);
        usuario.Ativar();

        return Result<Usuario>.Success(usuario);
    }

    public async Task<bool> SlugJaEmUsoAsync(string slug, CancellationToken cancellationToken = default)
    {
        var existente = await _usuarioRepo.GetBySlugAsync(slug, cancellationToken);
        return existente is not null;
    }

    private static Result<CPF> CriarCpf(string cpf) => CPF.Criar(cpf);
    private static Result<Email> CriarEmail(string email) => Email.Criar(email);
    private static Result<Telefone> CriarTelefone(string telefone) => Telefone.Criar(telefone);

    private async Task<bool> EmailDisponivelAsync(Email email, Guid? ignorarUsuarioId, CancellationToken cancellationToken)
    {
        var donoDoEmail = await _usuarioRepo.GetByEmailAsync(email, cancellationToken);
        if (donoDoEmail is null)
            return true;
        return donoDoEmail.Id == ignorarUsuarioId;
    }

    private async Task<Result<Usuario>> CriarNovoPassageiroAsync(
        string nome, CPF cpf, Email email, Telefone? telefone, string senha,
        CancellationToken cancellationToken)
    {
        var emailDisponivel = await EmailDisponivelAsync(email, null, cancellationToken);
        if (!emailDisponivel)
            return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        var usuario = Usuario.CriarPassageiro(nome, cpf, email, senhaHash, telefone);

        await _usuarioRepo.AddAsync(usuario, cancellationToken);

        return Result<Usuario>.Success(usuario);
    }

    private async Task<Result<Usuario>> ValidarEAtualizarExistenteAsync(
        Usuario usuario, string nome, Email email, string senha, Telefone? telefone,
        CancellationToken cancellationToken)
    {
        if (usuario.Email is null)
        {
            var emailDisponivel = await EmailDisponivelAsync(email, usuario.Id, cancellationToken);
            if (!emailDisponivel)
                return Error.Conflict("EMAIL_DUPLICADO", "Email já cadastrado.");

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
            usuario.AtualizarDados(nome, email, telefone);
            usuario.DefinirSenha(senhaHash);

            return Result<Usuario>.Success(usuario);
        }

        if (telefone is not null)
            usuario.AtualizarDados(usuario.Nome, usuario.Email, telefone);

        return Result<Usuario>.Success(usuario);
    }
}
