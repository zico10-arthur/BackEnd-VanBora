namespace VanBora.Application.DTOs.Reservas;

public class ReservaResponse
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public Guid ViagemVanId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public string CodigoPix { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
    public DateTime ExpiraEm { get; init; }
    public List<ItemReservaResponse> Itens { get; init; } = [];
}

public class ItemReservaResponse
{
    public Guid Id { get; init; }
    public int NumeroAssento { get; init; }
    public decimal PrecoAssento { get; init; }
    public string NomePassageiro { get; init; } = string.Empty;
}
