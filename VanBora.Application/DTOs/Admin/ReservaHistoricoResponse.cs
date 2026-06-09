namespace VanBora.Application.DTOs.Admin;

public class ReservaHistoricoResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public DateTime CriadaEm { get; init; }
    public ViagemResumoResponse Viagem { get; init; } = null!;
    public List<ItemReservaHistoricoResponse> Itens { get; init; } = [];
}

public class ViagemResumoResponse
{
    public Guid Id { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
}

public class ItemReservaHistoricoResponse
{
    public int Assento { get; init; }
    public string PassageiroNome { get; init; } = string.Empty;
    public string PassageiroDocumento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
}
