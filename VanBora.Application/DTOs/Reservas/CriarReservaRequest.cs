namespace VanBora.Application.DTOs.Reservas;

public sealed class CriarReservaRequest
{
    public Guid ViagemVanId { get; init; }
    public List<ItemReservaRequest> Itens { get; init; } = [];
}

public sealed class ItemReservaRequest
{
    public int NumeroAssento { get; init; }
    public string NomePassageiro { get; init; } = string.Empty;
    public string EmailPassageiro { get; init; } = string.Empty;
    public string TelefonePassageiro { get; init; } = string.Empty;
    public string CpfPassageiro { get; init; } = string.Empty;
}
