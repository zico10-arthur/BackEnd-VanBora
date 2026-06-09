namespace VanBora.Application.DTOs.Admin;

public class ViagemGerenteHistoricoResponse
{
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public DateTime DataEvento { get; init; }
    public int TotalReservas { get; init; }
    public decimal TotalArrecadado { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public string StatusViagem { get; init; } = string.Empty;
}
