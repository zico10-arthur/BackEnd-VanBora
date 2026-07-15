using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Domain.Services;

/// <summary>
///     Domain service responsável por calcular os indicadores do relatório financeiro
///     de uma viagem a partir das entidades de domínio.
/// </summary>
public static class RelatorioViagem
{
    public record PassageiroAssento(
        int NumeroAssento,
        string? NomePassageiro,
        string? TelefoneCompleto,
        string? StatusPagamento,
        string? VanPlaca);

    public record DadosRelatorio(
        int VansAlocadas,
        int TotalAssentos,
        int AssentosVendidos,
        int AssentosDisponiveis,
        int ReservasConfirmadas,
        decimal FaturamentoBruto,
        decimal TaxaPlataforma,
        decimal FaturamentoLiquido,
        bool BreakEvenAtingido,
        Guid? PrimeiraViagemVanId,
        IReadOnlyList<PassageiroAssento> Passageiros);

    private static readonly StatusReserva[] StatusEfetivos =
    [
        StatusReserva.Confirmada,
        StatusReserva.EmAndamento,
        StatusReserva.Concluida
    ];

    private static readonly StatusReserva[] StatusEmbarque =
    [
        StatusReserva.PendentePagamento,
        StatusReserva.Confirmada,
        StatusReserva.EmAndamento,
        StatusReserva.Concluida
    ];

    public static DadosRelatorio Calcular(Viagem viagem, List<Reserva> reservas)
    {
        var vansAtivas = viagem.ViagemVans
            .Where(vv => vv.Van is { Ativo: true })
            .OrderBy(vv => vv.Van!.Placa.Valor)
            .ToList();

        var vansAlocadas = vansAtivas.Count;
        var totalAssentos = vansAtivas.Sum(vv => vv.ObterQuantidadeAssentosParaReserva());

        var reservasEfetivas = reservas
            .Where(r => StatusEfetivos.Contains(r.Status))
            .ToList();

        var assentosVendidos = reservasEfetivas.Sum(r => r.Itens.Count);
        var assentosDisponiveis = Math.Max(0, totalAssentos - assentosVendidos);
        var reservasConfirmadas = reservasEfetivas.Count;
        var faturamentoBruto = reservasEfetivas.Sum(r => r.ValorTotal);
        var taxaPlataforma = reservasEfetivas.Sum(r => r.TaxaPlataforma);
        var faturamentoLiquido = faturamentoBruto - taxaPlataforma;
        var breakEvenAtingido = assentosVendidos >= viagem.QuorumMinimo;

        var ocupacaoPorVan = MontarOcupacaoPorVan(reservas);
        var passageiros = MontarListaEmbarque(vansAtivas, ocupacaoPorVan);
        var primeiraVanId = vansAtivas.FirstOrDefault()?.Id;

        return new DadosRelatorio(
            vansAlocadas,
            totalAssentos,
            assentosVendidos,
            assentosDisponiveis,
            reservasConfirmadas,
            faturamentoBruto,
            taxaPlataforma,
            faturamentoLiquido,
            breakEvenAtingido,
            primeiraVanId,
            passageiros);
    }

    private static Dictionary<(Guid ViagemVanId, int Assento), (Reserva ReservaAtual, ItemReserva Item)> MontarOcupacaoPorVan(
        List<Reserva> reservas)
    {
        var mapa = new Dictionary<(Guid, int), (Reserva, ItemReserva)>();

        foreach (var reserva in reservas.Where(r => StatusEmbarque.Contains(r.Status)))
        {
            if (reserva.Status == StatusReserva.PendentePagamento && reserva.EstaExpirada())
                continue;

            foreach (var item in reserva.Itens)
            {
                var key = (reserva.ViagemVanId, item.NumeroAssento);
                if (!mapa.TryGetValue(key, out var existente))
                {
                    mapa[key] = (reserva, item);
                    continue;
                }

                // Prefere reserva efetiva sobre pendente
                if (StatusEfetivos.Contains(reserva.Status) &&
                    !StatusEfetivos.Contains(existente.Item1.Status))
                {
                    mapa[key] = (reserva, item);
                }
            }
        }

        return mapa;
    }

    private static List<PassageiroAssento> MontarListaEmbarque(
        List<ViagemVan> vansAtivas,
        Dictionary<(Guid ViagemVanId, int Assento), (Reserva ReservaAtual, ItemReserva Item)> ocupacao)
    {
        var lista = new List<PassageiroAssento>();

        foreach (var vv in vansAtivas)
        {
            var capacidade = vv.ObterQuantidadeAssentosParaReserva();
            var placa = vv.Van.Placa.Valor;

            for (var assento = 1; assento <= capacidade; assento++)
            {
                if (ocupacao.TryGetValue((vv.Id, assento), out var ocupado))
                {
                    var statusPagamento = ocupado.ReservaAtual.Status switch
                    {
                        StatusReserva.PendentePagamento => "Pendente",
                        StatusReserva.Confirmada => "Confirmado",
                        StatusReserva.EmAndamento => "Confirmado",
                        StatusReserva.Concluida => "Confirmado",
                        _ => null
                    };

                    lista.Add(new PassageiroAssento(
                        assento,
                        ocupado.Item.NomePassageiro,
                        ocupado.Item.TelefonePassageiro.ValorCompleto,
                        statusPagamento,
                        placa));
                }
                else
                {
                    lista.Add(new PassageiroAssento(assento, null, null, null, placa));
                }
            }
        }

        return lista;
    }
}
