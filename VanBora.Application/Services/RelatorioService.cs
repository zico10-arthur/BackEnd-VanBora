using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;
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
        // 1. Buscar viagem com vans alocadas
        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<RelatorioResponse>.Failure(
                Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<RelatorioResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para acessar o relatório desta viagem."));

        // 2. Buscar todas as reservas da viagem
        var reservas = await _reservaRepo.GetByViagemIdAsync(viagemId, cancellationToken);

        // 3. Delegar o cálculo para o domain service
        var dados = RelatorioViagem.Calcular(viagem, reservas);

        // 4. Mapear para o DTO de resposta
        var response = new RelatorioResponse
        {
            VansAlocadas = dados.VansAlocadas,
            TotalAssentos = dados.TotalAssentos,
            AssentosVendidos = dados.AssentosVendidos,
            AssentosDisponiveis = dados.AssentosDisponiveis,
            ReservasConfirmadas = dados.ReservasConfirmadas,
            FaturamentoBruto = dados.FaturamentoBruto,
            TaxaPlataforma = dados.TaxaPlataforma,
            FaturamentoLiquido = dados.FaturamentoLiquido
        };

        return Result<RelatorioResponse>.Success(response);
    }
}
