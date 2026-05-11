using VanBora.Domain.Common;
using VanBora.Domain.Enums;

namespace VanBora.Domain.Entities;

public class Reserva
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid ViagemVanId { get; private set; }
    public StatusReserva Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public decimal TaxaPlataforma { get; private set; }
    public string CodigoPix { get; private set; }
    public string? TransacaoId { get; private set; }
    public DateTime? PagoEm { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }

    // Navigation properties
    public Usuario Usuario { get; private set; } = null!;
    public ViagemVan ViagemVan { get; private set; } = null!;

    private readonly List<ItemReserva> _itens = [];
    public IReadOnlyCollection<ItemReserva> Itens => _itens.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private Reserva() { }
#pragma warning restore CS8618

    public Reserva(
        Guid usuarioId,
        Guid viagemVanId,
        decimal valorTotal,
        decimal taxaPlataforma,
        string codigoPix)
    {
        Guard.AgainstEmptyGuid(usuarioId, nameof(usuarioId));
        Guard.AgainstEmptyGuid(viagemVanId, nameof(viagemVanId));
        Guard.AgainstNegativeOrZero(valorTotal, nameof(valorTotal));
        Guard.AgainstNegative(taxaPlataforma, nameof(taxaPlataforma));
        Guard.AgainstNullOrWhiteSpace(codigoPix, nameof(codigoPix));

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        ViagemVanId = viagemVanId;
        Status = StatusReserva.PendentePagamento;
        ValorTotal = valorTotal;
        TaxaPlataforma = taxaPlataforma;
        CodigoPix = codigoPix;
        CriadoEm = DateTime.UtcNow;
        ExpiraEm = CriadoEm.AddMinutes(10);
    }

    public void AdicionarItem(ItemReserva item)
    {
        Guard.AgainstNull(item, nameof(item));

        _itens.Add(item);
    }

    public void ConfirmarPagamento(string transacaoId)
    {
        Guard.AgainstInvalidState(Status == StatusReserva.PendentePagamento, "Apenas reservas pendentes de pagamento podem ser confirmadas.");
        Guard.AgainstNullOrWhiteSpace(transacaoId, nameof(transacaoId));

        Status = StatusReserva.Confirmada;
        TransacaoId = transacaoId;
        PagoEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        Guard.AgainstInvalidState(Status != StatusReserva.Concluida, "Reserva já concluída não pode ser cancelada.");
        Guard.AgainstInvalidState(Status != StatusReserva.Cancelada, "Reserva já está cancelada.");

        Status = StatusReserva.Cancelada;
    }

    public void IniciarViagem()
    {
        Guard.AgainstInvalidState(Status == StatusReserva.Confirmada, "Apenas reservas confirmadas podem iniciar a viagem.");

        Status = StatusReserva.EmAndamento;
    }

    public void Concluir()
    {
        Guard.AgainstInvalidState(Status == StatusReserva.EmAndamento, "Apenas reservas em andamento podem ser concluídas.");

        Status = StatusReserva.Concluida;
    }

    public void ExpiracaoAutomatica()
    {
        if (Status == StatusReserva.PendentePagamento && DateTime.UtcNow >= ExpiraEm)
        {
            Status = StatusReserva.Expirada;
        }
    }

    public bool EstaExpirada()
    {
        return Status == StatusReserva.PendentePagamento && DateTime.UtcNow >= ExpiraEm;
    }
}
