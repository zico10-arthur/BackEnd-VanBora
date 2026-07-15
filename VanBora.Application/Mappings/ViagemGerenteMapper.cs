using VanBora.Application.DTOs.Viagens;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Application.Mappings;

public static class ViagemGerenteMapper
{
    private static readonly StatusReserva[] StatusEfetivos =
    [
        StatusReserva.Confirmada,
        StatusReserva.EmAndamento,
        StatusReserva.Concluida
    ];

    public static ViagemGerenteResponse Map(
        Viagem viagem,
        IReadOnlyDictionary<Guid, int>? assentosVendidosPorVan = null,
        IReadOnlyList<Reserva>? reservas = null)
    {
        assentosVendidosPorVan ??= new Dictionary<Guid, int>();
        reservas ??= [];

        var reservasEfetivas = reservas
            .Where(r => StatusEfetivos.Contains(r.Status))
            .ToList();

        return new ViagemGerenteResponse
        {
            ViagemId = viagem.Id,
            NomeEvento = viagem.NomeEvento,
            DataEvento = viagem.DataEvento,
            LocalEvento = viagem.LocalEvento,
            DataPartida = viagem.DataPartida,
            LocalPartida = viagem.LocalPartida,
            PrecoAssento = viagem.PrecoAssento,
            QuorumMinimo = viagem.QuorumMinimo,
            PossuiIngresso = viagem.PossuiIngresso,
            Status = viagem.Status.ToString(),
            Receita = reservasEfetivas.Sum(r => r.ValorTotal),
            TotalReservas = reservasEfetivas.Count,
            Vans = viagem.ViagemVans
                .Where(vv => vv.Van is not null)
                .Select(vv =>
                {
                    assentosVendidosPorVan.TryGetValue(vv.Id, out var vendidos);
                    var capacidade = vv.ObterQuantidadeAssentosParaReserva();
                    return new ViagemVanGerenteInfo
                    {
                        ViagemVanId = vv.Id,
                        VanModelo = vv.Van.Modelo,
                        VanPlaca = vv.Van.Placa.Valor,
                        Capacidade = capacidade,
                        AssentosVendidos = vendidos,
                        MotoristaNome = vv.MotoristaUsuario?.Nome
                    };
                })
                .ToList()
        };
    }

    public static string MascararTelefone(string? telefoneCompleto)
    {
        if (string.IsNullOrWhiteSpace(telefoneCompleto))
            return string.Empty;

        var digits = new string(telefoneCompleto.Where(char.IsDigit).ToArray());
        if (digits.Length < 10)
            return "****";

        var ddd = digits[..2];
        var numero = digits[2..];
        var sufixo = numero.Length >= 4 ? numero[^4..] : numero;
        var prefixo = numero.Length > 4 ? numero[0].ToString() : "";
        return $"({ddd}) {prefixo}****-{sufixo}";
    }
}
