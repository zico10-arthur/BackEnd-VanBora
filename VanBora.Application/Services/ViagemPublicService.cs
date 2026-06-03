using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Interfaces;

namespace VanBora.Application.Services;

public class ViagemPublicService : IViagemPublicService
{
    private readonly IViagemRepository _viagemRepo;
    private readonly IViagemVanRepository _viagemVanRepo;
    private readonly IReservaRepository _reservaRepo;

    public ViagemPublicService(
        IViagemRepository viagemRepo,
        IViagemVanRepository viagemVanRepo,
        IReservaRepository reservaRepo)
    {
        _viagemRepo = viagemRepo;
        _viagemVanRepo = viagemVanRepo;
        _reservaRepo = reservaRepo;
    }

    public async Task<Result<List<ViagemPublicaResponse>>> ListarDisponiveisAsync(CancellationToken ct = default)
    {
        var viagens = await _viagemRepo.GetDisponiveisAsync(ct);
        var viagemVanIds = viagens.SelectMany(v => v.ViagemVans.Select(vv => vv.Id)).ToList();
        var ocupacaoPorVan = await _reservaRepo.GetAssentosOcupadosPorViagemVansAsync(viagemVanIds, ct);

        var lista = new List<ViagemPublicaResponse>();

        foreach (var viagem in viagens)
        {
            var vans = new List<ViagemVanPublicaResponse>();
            foreach (var vv in viagem.ViagemVans)
            {
                ocupacaoPorVan.TryGetValue(vv.Id, out var ocupados);
                ocupados ??= [];
                var cap = vv.ObterQuantidadeAssentosParaReserva();
                vans.Add(new ViagemVanPublicaResponse
                {
                    ViagemVanId = vv.Id,
                    ViagemId = viagem.Id,
                    NomeVan = vv.Van.Nome,
                    ModeloVan = vv.Van.Modelo,
                    PlacaVan = vv.Van.Placa.Valor,
                    CapacidadePassageiros = cap,
                    AssentosDisponiveis = Math.Max(0, cap - ocupados.Count)
                });
            }

            if (vans.Count == 0)
                continue;

            lista.Add(new ViagemPublicaResponse
            {
                ViagemId = viagem.Id,
                NomeEvento = viagem.NomeEvento,
                DataEvento = viagem.DataEvento,
                LocalEvento = viagem.LocalEvento,
                DataPartida = viagem.DataPartida,
                LocalPartida = viagem.LocalPartida,
                PrecoAssento = viagem.PrecoAssento,
                PossuiIngresso = viagem.PossuiIngresso,
                Status = viagem.Status.ToString(),
                Vans = vans
            });
        }

        return lista;
    }

    public async Task<Result<ViagemVanDetalheResponse>> ObterDetalheViagemVanAsync(
        Guid viagemVanId,
        CancellationToken ct = default)
    {
        var vv = await _viagemVanRepo.GetByIdAsync(viagemVanId, ct);
        if (vv is null)
            return Error.NotFound("VIAGEM_VAN_NAO_ENCONTRADA", "Viagem não encontrada.");

        var ocupados = await _reservaRepo.GetAssentosOcupadosAsync(viagemVanId, ct);

        return new ViagemVanDetalheResponse
        {
            ViagemVanId = vv.Id,
            ViagemId = vv.ViagemId,
            NomeEvento = vv.Viagem.NomeEvento,
            DataEvento = vv.Viagem.DataEvento,
            LocalEvento = vv.Viagem.LocalEvento,
            DataPartida = vv.Viagem.DataPartida,
            LocalPartida = vv.Viagem.LocalPartida,
            PrecoAssento = vv.Viagem.PrecoAssento,
            PossuiIngresso = vv.Viagem.PossuiIngresso,
            NomeVan = vv.Van.Nome,
            ModeloVan = vv.Van.Modelo,
            PlacaVan = vv.Van.Placa.Valor,
            CapacidadePassageiros = vv.ObterQuantidadeAssentosParaReserva(),
            AssentosOcupados = ocupados
        };
    }
}
