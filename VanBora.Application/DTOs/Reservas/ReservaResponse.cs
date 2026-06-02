namespace VanBora.Application.DTOs.Reservas;

public sealed class ReservaResponse
{
    public Guid Id { get; init; }
    public Guid ViagemVanId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public decimal ValorAPagar { get; init; }
    public string CodigoPix { get; init; } = string.Empty;
    public DateTime ExpiraEm { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime? PagoEm { get; init; }
    public List<ItemReservaResponse> Itens { get; init; } = [];
}

public sealed class ItemReservaResponse
{
    public Guid Id { get; init; }
    public int NumeroAssento { get; init; }
    public decimal PrecoAssento { get; init; }
    public string NomePassageiro { get; init; } = string.Empty;
}

public sealed class PagarReservaResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string InitPoint { get; init; } = string.Empty;
    public string? SandboxInitPoint { get; init; }
    public string PreferenceId { get; init; } = string.Empty;
    public decimal ValorAPagar { get; init; }
    public DateTime ExpiraEm { get; init; }
}
