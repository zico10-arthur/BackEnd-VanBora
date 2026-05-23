using VanBora.Domain.Common;

namespace VanBora.Domain.Entities;

public class ViagemVan
{
    public Guid Id { get; private set; }
    public Guid ViagemId { get; private set; }
    public Guid VanId { get; private set; }
    public Guid? MotoristaUsuarioId { get; private set; }

    // Navigation properties
    public Viagem Viagem { get; private set; } = null!;
    public Van Van { get; private set; } = null!;
    public Usuario? MotoristaUsuario { get; private set; }

    private readonly List<Reserva> _reservas = [];
    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private ViagemVan() { }
#pragma warning restore CS8618

    public ViagemVan(Guid viagemId, Guid vanId)
    {
        Guard.AgainstEmptyGuid(viagemId, nameof(viagemId));
        Guard.AgainstEmptyGuid(vanId, nameof(vanId));

        Id = Guid.NewGuid();
        ViagemId = viagemId;
        VanId = vanId;
    }

    public void AlocarMotorista(Guid motoristaUsuarioId)
    {
        Guard.AgainstEmptyGuid(motoristaUsuarioId, nameof(motoristaUsuarioId));

        MotoristaUsuarioId = motoristaUsuarioId;
    }

    public void DesalocarMotorista()
    {
        MotoristaUsuarioId = null;
    }

    public int ObterQuantidadeAssentosParaReserva()
    {
        // Capacidade total - 1 (motorista) = assentos disponíveis para reserva
        return Van.Capacidade - 1;
    }
}
