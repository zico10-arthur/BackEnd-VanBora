using VanBora.Domain.Common;

namespace VanBora.Domain.Entities;

public class ViagemVan
{
    public Guid Id { get; private set; }
    public Guid ViagemId { get; private set; }
    public Guid VanId { get; private set; }
    public Guid? MotoristaPerfilId { get; private set; }

    // Navigation properties
    public Viagem Viagem { get; private set; } = null!;
    public Van Van { get; private set; } = null!;
    public Perfil? MotoristaPerfil { get; private set; }

    private readonly List<Reserva> _reservas = [];
    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private ViagemVan() { }
#pragma warning restore CS8618

    public ViagemVan(Guid viagemId, Van van)
    {
        Guard.AgainstEmptyGuid(viagemId, nameof(viagemId));
        Guard.AgainstNull(van, nameof(van));
        Guard.AgainstInvalidState(van.Ativo, "Não é possível alocar uma van inativa a uma viagem.");

        Id = Guid.NewGuid();
        ViagemId = viagemId;
        VanId = van.Id;
        Van = van;
    }

    public void AlocarMotorista(Guid motoristaPerfilId)
    {
        Guard.AgainstEmptyGuid(motoristaPerfilId, nameof(motoristaPerfilId));

        MotoristaPerfilId = motoristaPerfilId;
    }

    public void DesalocarMotorista()
    {
        MotoristaPerfilId = null;
    }

    public int ObterQuantidadeAssentosParaReserva()
    {
        // Capacidade total - 1 (motorista) = assentos disponíveis para reserva
        return Van.Capacidade - 1;
    }
}
