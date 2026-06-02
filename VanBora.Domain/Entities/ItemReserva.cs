using VanBora.Domain.Common;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Entities;

public class ItemReserva
{
    public Guid Id { get; private set; }
    public Guid ReservaId { get; private set; }
    public int NumeroAssento { get; private set; }
    public Dinheiro PrecoAssento { get; private set; }
    public string NomePassageiro { get; private set; }
    public Email EmailPassageiro { get; private set; }
    public Telefone TelefonePassageiro { get; private set; }
    public CPF CPFPassageiro { get; private set; }

    // Navigation property
    public Reserva Reserva { get; private set; } = null!;

#pragma warning disable CS8618 // EF Core constructor
    private ItemReserva() { }
#pragma warning restore CS8618

    public ItemReserva(
        int numeroAssento,
        Dinheiro precoAssento,
        string nomePassageiro,
        Email emailPassageiro,
        Telefone telefonePassageiro,
        CPF cpfPassageiro)
    {
        Guard.AgainstNegativeOrZero(numeroAssento, nameof(numeroAssento));
        Guard.AgainstNull(precoAssento, nameof(precoAssento));
        Guard.AgainstNullOrWhiteSpace(nomePassageiro, nameof(nomePassageiro));
        Guard.AgainstNull(emailPassageiro, nameof(emailPassageiro));
        Guard.AgainstNull(telefonePassageiro, nameof(telefonePassageiro));
        Guard.AgainstNull(cpfPassageiro, nameof(cpfPassageiro));

        Id = Guid.NewGuid();
        NumeroAssento = numeroAssento;
        PrecoAssento = precoAssento;
        NomePassageiro = nomePassageiro;
        EmailPassageiro = emailPassageiro;
        TelefonePassageiro = telefonePassageiro;
        CPFPassageiro = cpfPassageiro;
    }

    internal void VincularReserva(Guid reservaId)
    {
        Guard.AgainstEmptyGuid(reservaId, nameof(reservaId));
        ReservaId = reservaId;
    }
}
