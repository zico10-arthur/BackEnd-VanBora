using VanBora.Domain.Common;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Entities;

public class Van
{
    public Guid Id { get; private set; }
    public Guid GerenteUsuarioId { get; private set; }
    public string Nome { get; private set; }
    public Placa Placa { get; private set; }
    public string Modelo { get; private set; }
    public int Capacidade { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    // Navigation properties
    public Usuario GerenteUsuario { get; private set; } = null!;
    private readonly List<ViagemVan> _viagemVans = [];
    public IReadOnlyCollection<ViagemVan> ViagemVans => _viagemVans.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private Van() { }
#pragma warning restore CS8618

    public Van(Guid gerenteUsuarioId, string nome, Placa placa, string modelo, int capacidade)
    {
        Guard.AgainstEmptyGuid(gerenteUsuarioId, nameof(gerenteUsuarioId));
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(placa, nameof(placa));
        Guard.AgainstNullOrWhiteSpace(modelo, nameof(modelo));
        Guard.AgainstLessThan(capacidade, 2, nameof(capacidade)); // mínimo: 1 motorista + 1 passageiro

        Id = Guid.NewGuid();
        GerenteUsuarioId = gerenteUsuarioId;
        Nome = nome;
        Placa = placa;
        Modelo = modelo;
        Capacidade = capacidade;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarDados(string nome, Placa placa, string modelo)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(placa, nameof(placa));
        Guard.AgainstNullOrWhiteSpace(modelo, nameof(modelo));

        Nome = nome;
        Placa = placa;
        Modelo = modelo;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public int ObterQuantidadeAssentosDisponiveis()
    {
        // Capacidade inclui motorista, então assentos para reserva = Capacidade - 1
        return Capacidade - 1;
    }
}
