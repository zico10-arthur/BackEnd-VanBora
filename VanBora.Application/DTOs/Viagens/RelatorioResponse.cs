namespace VanBora.Application.DTOs.Viagens;

public class RelatorioResponse
{
    public int VansAlocadas { get; init; }
    public int TotalAssentos { get; init; }
    public int AssentosVendidos { get; init; }
    public int AssentosDisponiveis { get; init; }
    public int ReservasConfirmadas { get; init; }
    public decimal FaturamentoBruto { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public decimal FaturamentoLiquido { get; init; }
}
