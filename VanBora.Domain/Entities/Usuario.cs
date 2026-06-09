using VanBora.Domain.Common;
using VanBora.Domain.Enums;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Entities;

public class Usuario
{
    // Identity (todo usuário)
    public Guid Id { get; private set; }
    public TipoUsuario Tipo { get; private set; }
    public string Nome { get; private set; }
    public CPF CPF { get; private set; }
    public Email? Email { get; private set; }
    public string? SenhaHash { get; private set; }
    public Telefone? Telefone { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    // Gerente-specific (nullable, only when Tipo == Gerente)
    public string? Slug { get; private set; }
    public decimal? TaxaPlataforma { get; private set; }
    public bool? Gratuito { get; private set; }
    public string? ChavePix { get; private set; }

    // Motorista-specific (nullable, only when Tipo == Motorista)
    public CNH? CNH { get; private set; }
    public Guid? CriadoPorUsuarioId { get; private set; }

    // Exclusão de conta (US20)
    public string? CodigoExclusao { get; private set; }
    public DateTime? CodigoExclusaoExpiraEm { get; private set; }

    // Navigation — APENAS auto-relacionamento Motorista → Gerente
    public Usuario? CriadoPorUsuario { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
    private Usuario() { }
#pragma warning restore CS8618

    // --- Factory Methods ---

    public static Usuario CriarPassageiro(
        string nome,
        CPF cpf,
        Email email,
        string senhaHash,
        Telefone? telefone)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(cpf, nameof(cpf));
        Guard.AgainstNull(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(senhaHash, nameof(senhaHash));

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Tipo = TipoUsuario.Passageiro,
            Nome = nome,
            CPF = cpf,
            Email = email,
            SenhaHash = senhaHash,
            Telefone = telefone,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    public static Usuario CriarGerente(
        string nome,
        CPF cpf,
        Email email,
        string senhaHash,
        Telefone? telefone,
        string slug,
        decimal taxaPlataforma,
        bool gratuito,
        string? chavePix)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(cpf, nameof(cpf));
        Guard.AgainstNull(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(senhaHash, nameof(senhaHash));
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));
        Guard.AgainstNegative(taxaPlataforma, nameof(taxaPlataforma));

        var slugNormalizado = slug.Trim().ToLowerInvariant();

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Tipo = TipoUsuario.Gerente,
            Nome = nome,
            CPF = cpf,
            Email = email,
            SenhaHash = senhaHash,
            Telefone = telefone,
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Slug = slugNormalizado,
            TaxaPlataforma = taxaPlataforma,
            Gratuito = gratuito,
            ChavePix = chavePix
        };
    }

    public static Usuario CriarMotorista(
        string nome,
        CPF cpf,
        Telefone? telefone,
        CNH cnh,
        Guid criadoPorUsuarioId)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(cpf, nameof(cpf));
        Guard.AgainstNull(cnh, nameof(cnh));
        Guard.AgainstEmptyGuid(criadoPorUsuarioId, nameof(criadoPorUsuarioId));

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Tipo = TipoUsuario.Motorista,
            Nome = nome,
            CPF = cpf,
            Telefone = telefone,
            // Email = null, SenhaHash = null (sem login)
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            CNH = cnh,
            CriadoPorUsuarioId = criadoPorUsuarioId
        };
    }

    public static Usuario CriarAdmin(
        string nome,
        CPF cpf,
        Email email,
        string senhaHash)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(cpf, nameof(cpf));
        Guard.AgainstNull(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(senhaHash, nameof(senhaHash));

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Tipo = TipoUsuario.Admin,
            Nome = nome,
            CPF = cpf,
            Email = email,
            SenhaHash = senhaHash,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    // --- Domain Methods ---

    public void AtualizarDados(string nome, Email? email, Telefone? telefone)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));

        Nome = nome;
        Email = email ?? Email;
        Telefone = telefone ?? Telefone;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirSenha(string senhaHash)
    {
        Guard.AgainstNullOrWhiteSpace(senhaHash, nameof(senhaHash));
        SenhaHash = senhaHash;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void UpgradeParaGerente(string slug, decimal taxaPlataforma, bool gratuito, string? chavePix)
    {
        Guard.AgainstInvalidState(Tipo == TipoUsuario.Passageiro, "Apenas Passageiros podem fazer upgrade para Gerente.");
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));
        Guard.AgainstNegative(taxaPlataforma, nameof(taxaPlataforma));

        Tipo = TipoUsuario.Gerente;
        Slug = slug.Trim().ToLowerInvariant();
        TaxaPlataforma = taxaPlataforma;
        Gratuito = gratuito;
        ChavePix = chavePix;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarSlug(string slug)
    {
        Guard.AgainstInvalidState(Tipo == TipoUsuario.Gerente, "Apenas Gerentes podem alterar o slug.");
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));

        Slug = slug.Trim().ToLowerInvariant();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarParametrosGerente(decimal? taxaPlataforma, bool? gratuito)
    {
        Guard.AgainstInvalidState(Tipo == TipoUsuario.Gerente, "Apenas Gerentes possuem taxa.");
        if (taxaPlataforma.HasValue)
        {
            Guard.AgainstNegative(taxaPlataforma.Value, nameof(taxaPlataforma));
            TaxaPlataforma = taxaPlataforma.Value;
        }
        if (gratuito.HasValue)
            Gratuito = gratuito.Value;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirCodigoExclusao(string codigo, DateTime expiraEm)
    {
        Guard.AgainstNullOrWhiteSpace(codigo, nameof(codigo));
        CodigoExclusao = codigo;
        CodigoExclusaoExpiraEm = expiraEm;
    }

    public void LimparCodigoExclusao()
    {
        CodigoExclusao = null;
        CodigoExclusaoExpiraEm = null;
    }

    public void AtualizarChavePix(string? chavePix)
    {
        Guard.AgainstInvalidState(Tipo == TipoUsuario.Gerente, "Apenas Gerentes podem alterar a chave PIX.");

        ChavePix = chavePix;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void RegistrarCNH(CNH cnh)
    {
        Guard.AgainstInvalidState(Tipo == TipoUsuario.Motorista, "Apenas Motoristas podem registrar CNH.");
        Guard.AgainstNull(cnh, nameof(cnh));

        CNH = cnh;
        DataAtualizacao = DateTime.UtcNow;
    }
}
