using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Domain.Services;

/// <summary>
///     Domain service responsável por calcular os indicadores do relatório financeiro
///     de uma viagem a partir das entidades de domínio.
/// </summary>
public static class RelatorioViagem
{
    /// <summary>
    ///     Dados calculados do relatório.
    /// </summary>
    public record DadosRelatorio(
        int VansAlocadas,
        int TotalAssentos,
        int AssentosVendidos,
        int AssentosDisponiveis,
        int ReservasConfirmadas,
        decimal FaturamentoBruto,
        decimal TaxaPlataforma,
        decimal FaturamentoLiquido);

    /// <summary>
    ///     Calcula os indicadores do relatório financeiro de uma viagem.
    /// </summary>
    /// <param name="viagem">Entidade Viagem com ViagemVans e Vans carregadas.</param>
    /// <param name="reservas">Lista de reservas da viagem.</param>
    /// <returns>Dados do relatório financeiro.</returns>
    public static DadosRelatorio Calcular(Viagem viagem, List<Reserva> reservas)
    {
        var vansAtivas = viagem.ViagemVans
            .Where(vv => vv.Van is { Ativo: true })
            .ToList();

        var vansAlocadas = vansAtivas.Count;
        var totalAssentos = vansAtivas.Sum(vv => vv.Van!.Capacidade - 1);

        // Considera apenas reservas com pagamento confirmado ou que já utilizaram o serviço
        var reservasEfetivas = reservas
            .Where(r => r.Status is StatusReserva.Confirmada
                or StatusReserva.EmAndamento
                or StatusReserva.Concluida)
            .ToList();

        var assentosVendidos = reservasEfetivas.Sum(r => r.Itens.Count);
        var assentosDisponiveis = totalAssentos - assentosVendidos;
        var reservasConfirmadas = reservasEfetivas.Count;
        var faturamentoBruto = reservasEfetivas.Sum(r => r.ValorTotal);
        var taxaPlataforma = reservasEfetivas.Sum(r => r.TaxaPlataforma);
        var faturamentoLiquido = faturamentoBruto - taxaPlataforma;

        return new DadosRelatorio(
            vansAlocadas,
            totalAssentos,
            assentosVendidos,
            assentosDisponiveis,
            reservasConfirmadas,
            faturamentoBruto,
            taxaPlataforma,
            faturamentoLiquido);
    }
}
