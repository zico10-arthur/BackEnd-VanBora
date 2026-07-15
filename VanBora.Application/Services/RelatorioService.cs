using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;
using VanBora.Application.Mappings;
using VanBora.Domain.Common;
using VanBora.Domain.Interfaces;
using VanBora.Domain.Services;

namespace VanBora.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IViagemRepository _viagemRepo;
    private readonly IReservaRepository _reservaRepo;

    public RelatorioService(
        IViagemRepository viagemRepo,
        IReservaRepository reservaRepo)
    {
        _viagemRepo = viagemRepo;
        _reservaRepo = reservaRepo;
    }

    public async Task<Result<RelatorioResponse>> GerarRelatorioAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdReadOnlyAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<RelatorioResponse>.Failure(
                Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<RelatorioResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para acessar o relatório desta viagem."));

        var reservas = await _reservaRepo.GetByViagemIdAsync(viagemId, cancellationToken);
        var dados = RelatorioViagem.Calcular(viagem, reservas);

        var response = new RelatorioResponse
        {
            ViagemId = viagem.Id,
            NomeEvento = viagem.NomeEvento,
            DataEvento = viagem.DataEvento,
            Origem = viagem.LocalPartida,
            Destino = viagem.LocalEvento,
            Status = viagem.Status.ToString(),
            ReceitaTotal = dados.FaturamentoBruto,
            TaxaPlataforma = dados.TaxaPlataforma,
            FaturamentoLiquido = dados.FaturamentoLiquido,
            AssentosVendidos = dados.AssentosVendidos,
            CapacidadeTotal = dados.TotalAssentos,
            QuorumMinimo = viagem.QuorumMinimo,
            PrecoAssento = viagem.PrecoAssento,
            BreakEvenAtingido = dados.BreakEvenAtingido,
            ViagemVanId = dados.PrimeiraViagemVanId,
            VansAlocadas = dados.VansAlocadas,
            AssentosDisponiveis = dados.AssentosDisponiveis,
            ReservasConfirmadas = dados.ReservasConfirmadas,
            Passageiros = dados.Passageiros
                .Select(p => new PassageiroRelatorioResponse
                {
                    NumeroAssento = p.NumeroAssento,
                    NomePassageiro = p.NomePassageiro,
                    TelefonePassageiro = string.IsNullOrEmpty(p.TelefoneCompleto)
                        ? null
                        : ViagemGerenteMapper.MascararTelefone(p.TelefoneCompleto),
                    StatusPagamento = p.StatusPagamento,
                    VanPlaca = p.VanPlaca
                })
                .ToList()
        };

        return Result<RelatorioResponse>.Success(response);
    }
}
