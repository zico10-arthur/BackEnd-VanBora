namespace VanBora.Application.DTOs.Viagens;

public class RelatorioResponse
{
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; }
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal ReceitaTotal { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public decimal FaturamentoLiquido { get; init; }
    public int AssentosVendidos { get; init; }
    public int CapacidadeTotal { get; init; }
    public int QuorumMinimo { get; init; }
    public decimal PrecoAssento { get; init; }
    public bool BreakEvenAtingido { get; init; }
    public Guid? ViagemVanId { get; init; }
    public int VansAlocadas { get; init; }
    public int AssentosDisponiveis { get; init; }
    public int ReservasConfirmadas { get; init; }
    public List<PassageiroRelatorioResponse> Passageiros { get; init; } = [];
}

public class PassageiroRelatorioResponse
{
    public int NumeroAssento { get; init; }
    public string? NomePassageiro { get; init; }
    public string? TelefonePassageiro { get; init; }
    public string? StatusPagamento { get; init; }
    public string? VanPlaca { get; init; }
}
