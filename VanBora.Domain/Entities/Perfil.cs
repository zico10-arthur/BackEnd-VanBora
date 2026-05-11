using VanBora.Domain.Common;
using VanBora.Domain.Enums;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Entities;

public class Perfil
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public TipoPerfil Tipo { get; private set; }
    public bool Ativo { get; private set; }
    public Guid? CriadoPorPerfilId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    // Propriedades específicas do Gerente
    public string? Slug { get; private set; }
    public decimal? TaxaPlataforma { get; private set; }
    public bool? Gratuito { get; private set; }

    // Propriedade específica do Motorista
    public CNH? CNH { get; private set; }

    // Navigation properties
    public Usuario Usuario { get; private set; } = null!;
    public Perfil? CriadoPorPerfil { get; private set; }

    private readonly List<Perfil> _motoristasCriados = [];
    public IReadOnlyCollection<Perfil> MotoristasCriados => _motoristasCriados.AsReadOnly();

    private readonly List<Van> _vans = [];
    public IReadOnlyCollection<Van> Vans => _vans.AsReadOnly();

    private readonly List<Viagem> _viagens = [];
    public IReadOnlyCollection<Viagem> Viagens => _viagens.AsReadOnly();

    private readonly List<ViagemVan> _viagemVansDirigindo = [];
    public IReadOnlyCollection<ViagemVan> ViagemVansDirigindo => _viagemVansDirigindo.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private Perfil() { }
#pragma warning restore CS8618

    private Perfil(Guid usuarioId, TipoPerfil tipo, Guid? criadoPorPerfilId)
    {
        Guard.AgainstEmptyGuid(usuarioId, nameof(usuarioId));

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Tipo = tipo;
        Ativo = true;
        CriadoPorPerfilId = criadoPorPerfilId;
        CriadoEm = DateTime.UtcNow;
    }

    public static Perfil CriarPassageiro(Guid usuarioId)
    {
        return new Perfil(usuarioId, TipoPerfil.Passageiro, null);
    }

    public static Perfil CriarGerente(Guid usuarioId, string slug, decimal taxaPlataforma, bool gratuito)
    {
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));
        Guard.AgainstNegative(taxaPlataforma, nameof(taxaPlataforma));

        slug = slug.Trim().ToLowerInvariant();

        return new Perfil(usuarioId, TipoPerfil.Gerente, null)
        {
            Slug = slug,
            TaxaPlataforma = taxaPlataforma,
            Gratuito = gratuito
        };
    }

    public static Perfil CriarMotorista(Guid usuarioId, Guid criadoPorPerfilId, CNH cnh)
    {
        Guard.AgainstEmptyGuid(criadoPorPerfilId, nameof(criadoPorPerfilId));
        Guard.AgainstNull(cnh, nameof(cnh));

        return new Perfil(usuarioId, TipoPerfil.Motorista, criadoPorPerfilId)
        {
            CNH = cnh
        };
    }

    public static Perfil CriarAdmin(Guid usuarioId)
    {
        return new Perfil(usuarioId, TipoPerfil.Admin, null);
    }

    public void AtualizarSlug(string slug)
    {
        Guard.AgainstInvalidState(Tipo == TipoPerfil.Gerente, "Apenas perfis do tipo Gerente possuem slug.");
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));

        Slug = slug.Trim().ToLowerInvariant();
    }

    public void AtualizarTaxa(decimal taxaPlataforma, bool gratuito)
    {
        Guard.AgainstInvalidState(Tipo == TipoPerfil.Gerente, "Apenas perfis do tipo Gerente possuem taxa.");
        Guard.AgainstNegative(taxaPlataforma, nameof(taxaPlataforma));

        TaxaPlataforma = taxaPlataforma;
        Gratuito = gratuito;
    }

    public void AtualizarCNH(CNH cnh)
    {
        Guard.AgainstInvalidState(Tipo == TipoPerfil.Motorista, "Apenas perfis do tipo Motorista possuem CNH.");
        Guard.AgainstNull(cnh, nameof(cnh));

        CNH = cnh;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }
}
