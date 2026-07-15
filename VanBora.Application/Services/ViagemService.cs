using AutoMapper;
using FluentValidation;
using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;
using VanBora.Application.Mappings;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;

namespace VanBora.Application.Services;

public class ViagemService : IViagemService
{
    private readonly IViagemRepository _viagemRepo;
    private readonly IVanRepository _vanRepo;
    private readonly IViagemVanRepository _viagemVanRepo;
    private readonly IReservaRepository _reservaRepo;
    private readonly IMapper _mapper;
    private readonly IValidator<CriarViagemRequest> _criarValidator;
    private readonly IValidator<AtualizarViagemRequest> _atualizarValidator;
    private readonly IValidator<AlocarVanRequest> _alocarVanValidator;

    private readonly IValidator<AlocarMotoristaRequest> _alocarMotoristaValidator;

    private readonly IUsuarioRepository _repository;

    public ViagemService(
        IViagemRepository viagemRepo,
        IVanRepository vanRepo,
        IViagemVanRepository viagemVanRepo,
        IReservaRepository reservaRepo,
        IMapper mapper,
        IValidator<CriarViagemRequest> criarValidator,
        IValidator<AtualizarViagemRequest> atualizarValidator,
        IValidator<AlocarVanRequest> alocarVanValidator,
        IValidator<AlocarMotoristaRequest> alocarMotoristaValidator,
        IUsuarioRepository repository)
    {
        _viagemRepo = viagemRepo;
        _vanRepo = vanRepo;
        _viagemVanRepo = viagemVanRepo;
        _reservaRepo = reservaRepo;
        _mapper = mapper;
        _criarValidator = criarValidator;
        _atualizarValidator = atualizarValidator;
        _alocarVanValidator = alocarVanValidator;
        _alocarMotoristaValidator = alocarMotoristaValidator;
        _repository = repository;
    }

    public async Task<Result<ViagemResponse>> CriarAsync(
        Guid gerenteUsuarioId,
        CriarViagemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _criarValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<ViagemResponse>.Failure(Error.Validation(erros));
        }

        var viagem = new Viagem(
            gerenteUsuarioId,
            request.NomeEvento,
            request.DataEvento,
            request.LocalEvento,
            request.DataPartida,
            request.LocalPartida,
            request.PrecoAssento,
            request.PossuiIngresso,
            request.QuorumMinimo);

        await _viagemRepo.AddAsync(viagem, cancellationToken);
        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ViagemResponse>(viagem);
        return Result<ViagemResponse>.Success(response);
    }

    public async Task<Result<ViagemResponse>> ObterPorIdAsync(
        Guid viagemId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);

        if (viagem is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("ViagemNotFound", "Viagem não encontrada."));

        var response = _mapper.Map<ViagemResponse>(viagem);
        return Result<ViagemResponse>.Success(response);
    }

    public async Task<Result<ViagemGerenteResponse>> ObterPorGerenteAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdReadOnlyAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<ViagemGerenteResponse>.Failure(
                Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<ViagemGerenteResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para acessar esta viagem."));

        return Result<ViagemGerenteResponse>.Success(
            await MapearGerenteComEstatisticasAsync(viagem, cancellationToken));
    }

    public async Task<Result<List<ViagemResponse>>> ListarDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        var viagens = await _viagemRepo.GetDisponiveisAsync(cancellationToken);

        var response = _mapper.Map<List<ViagemResponse>>(viagens);
        return Result<List<ViagemResponse>>.Success(response);
    }

    public async Task<Result<List<ViagemGerenteResponse>>> ListarPorGerenteAsync(
        Guid gerenteUsuarioId,
        CancellationToken cancellationToken = default)
    {
        var viagens = await _viagemRepo.GetByGerenteUsuarioIdAsync(gerenteUsuarioId, cancellationToken);
        var viagemVanIds = viagens.SelectMany(v => v.ViagemVans.Select(vv => vv.Id)).ToList();
        var reservas = await _reservaRepo.GetByViagemVanIdsAsync(viagemVanIds, cancellationToken);
        var ocupacao = await _reservaRepo.GetAssentosOcupadosPorViagemVansAsync(viagemVanIds, cancellationToken);

        var assentosVendidosPorVan = ocupacao.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Count);

        var reservasPorViagem = reservas
            .GroupBy(r => r.ViagemVanId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var response = viagens
            .Select(viagem =>
            {
                var reservasDaViagem = viagem.ViagemVans
                    .SelectMany(vv =>
                        reservasPorViagem.TryGetValue(vv.Id, out var list) ? list : [])
                    .ToList();

                return ViagemGerenteMapper.Map(viagem, assentosVendidosPorVan, reservasDaViagem);
            })
            .ToList();

        return Result<List<ViagemGerenteResponse>>.Success(response);
    }

    private async Task<ViagemGerenteResponse> MapearGerenteComEstatisticasAsync(
        Viagem viagem,
        CancellationToken cancellationToken)
    {
        var viagemVanIds = viagem.ViagemVans.Select(vv => vv.Id).ToList();
        var reservas = await _reservaRepo.GetByViagemVanIdsAsync(viagemVanIds, cancellationToken);
        var ocupacao = await _reservaRepo.GetAssentosOcupadosPorViagemVansAsync(viagemVanIds, cancellationToken);
        var assentosVendidosPorVan = ocupacao.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
        return ViagemGerenteMapper.Map(viagem, assentosVendidosPorVan, reservas);
    }

    public async Task<Result<ViagemResponse>> AtualizarAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        AtualizarViagemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _atualizarValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<ViagemResponse>.Failure(Error.Validation(erros));
        }

        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<ViagemResponse>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para alterar esta viagem."));

        viagem.AtualizarDados(
            request.NomeEvento,
            request.DataEvento,
            request.LocalEvento,
            request.DataPartida,
            request.LocalPartida,
            request.PossuiIngresso);

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ViagemResponse>(viagem);
        return Result<ViagemResponse>.Success(response);
    }

    public async Task<Result<bool>> CancelarAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<bool>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<bool>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para cancelar esta viagem."));

        if (viagem.Status == StatusViagem.Concluida)
            return Result<bool>.Failure(Error.Validation("CANCELAMENTO_INVALIDO", "Viagem já concluída não pode ser cancelada."));

        if (viagem.Status == StatusViagem.Cancelada)
            return Result<bool>.Failure(Error.Validation("CANCELAMENTO_INVALIDO", "Viagem já está cancelada."));

        viagem.Cancelar();

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ViagemResponse>> AlocarVanAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        AlocarVanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _alocarVanValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<ViagemResponse>.Failure(Error.Validation(erros));
        }

        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<ViagemResponse>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para alterar esta viagem."));

        if (viagem.Status != StatusViagem.Agendada)
            return Result<ViagemResponse>.Failure(Error.Validation("STATUS_INVALIDO", "Apenas viagens agendadas podem receber vans."));

        var van = await _vanRepo.GetByIdAndGerenteAsync(request.VanId, gerenteUsuarioId, cancellationToken);
        if (van is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("VAN_NAO_ENCONTRADA", "Van não encontrada."));

        if (!van.Ativo)
            return Result<ViagemResponse>.Failure(Error.Validation("VAN_INATIVA", "Não é possível alocar uma van inativa a uma viagem."));

        if (viagem.ViagemVans.Any(vv => vv.VanId == request.VanId))
            return Result<ViagemResponse>.Failure(Error.Validation("VAN_JA_ALOCADA", "Esta van já está alocada nesta viagem."));

        if (van.Capacidade < viagem.QuorumMinimo)
            return Result<ViagemResponse>.Failure(
                Error.Validation("CAPACIDADE_INSUFICIENTE", $"A van '{van.Nome}' tem capacidade de {van.Capacidade} lugares, mas o quórum mínimo da viagem é {viagem.QuorumMinimo}."));

        // Verifica se a van já está alocada em outra viagem com conflito de horário (< 12h de diferença)
        var alocacoesExistentes = await _viagemVanRepo.GetByVanIdAsync(request.VanId, cancellationToken);
        var conflito = alocacoesExistentes
            .Where(vv => vv.ViagemId != viagemId)
            .Where(vv => vv.Viagem.Status != StatusViagem.Cancelada)
            .Any(vv => Math.Abs((vv.Viagem.DataPartida - viagem.DataPartida).TotalHours) < 12);

        if (conflito)
            return Result<ViagemResponse>.Failure(
                Error.Validation("CONFLITO_HORARIO", "Esta van já está alocada em outra viagem com horário conflitante (menos de 12 horas de diferença)."));

        var viagemVan = new ViagemVan(viagemId, request.VanId);
        viagem.AdicionarViagemVan(viagemVan);

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ViagemResponse>(viagem);
        return Result<ViagemResponse>.Success(response);
    }

    public async Task<Result<bool>> RemoverVanAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        Guid viagemVanId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<bool>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<bool>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para alterar esta viagem."));

        if (viagem.Status != StatusViagem.Agendada)
            return Result<bool>.Failure(Error.Validation("STATUS_INVALIDO", "Apenas viagens agendadas podem ter vans removidas."));

        var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == viagemVanId);
        if (viagemVan is null)
            return Result<bool>.Failure(Error.NotFound("VAN_NAO_ALOCADA", "Esta van não está alocada nesta viagem."));

        viagem.RemoverViagemVan(viagemVan);
        _viagemVanRepo.Remove(viagemVan);

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

     public async Task<Result<ViagemResponse>> AlocarMotoristaAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        AlocarMotoristaRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _alocarMotoristaValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<ViagemResponse>.Failure(Error.Validation(erros));
        }

        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<ViagemResponse>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para alterar esta viagem."));

        if (viagem.Status != StatusViagem.Agendada)
            return Result<ViagemResponse>.Failure(Error.Validation("STATUS_INVALIDO", "Apenas viagens agendadas podem receber motoristas."));

        var motorista = await _repository.GetByIdAsync(request.MotoristaId, cancellationToken);
        if (motorista is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("MOTORISTA_NAO_ENCONTRADO", "Motorista não encontrado."));

        if (motorista.Tipo != TipoUsuario.Motorista)
            return Result<ViagemResponse>.Failure(Error.Validation("USUARIO_NAO_E_MOTORISTA", "O usuário informado não é um motorista."));

        if (!motorista.Ativo)
            return Result<ViagemResponse>.Failure(Error.Validation("MOTORISTA_INATIVO", "Não é possível alocar um motorista inativo a uma viagem."));

        if (motorista.CriadoPorUsuarioId != gerenteUsuarioId)
            return Result<ViagemResponse>.Failure(Error.NotFound("MOTORISTA_DE_OUTRO_GERENTE", "Motorista não pertence a este gerente."));

        var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == request.ViagemVanId);
        if (viagemVan is null)
            return Result<ViagemResponse>.Failure(Error.NotFound("VIAGEMVAN_NAO_ENCONTRADA", "Vínculo viagem-van não encontrado."));

        if (viagem.ViagemVans.Any(vv => vv.MotoristaUsuarioId == request.MotoristaId))
            return Result<ViagemResponse>.Failure(Error.Validation("MOTORISTA_JA_ALOCADO", "Este motorista já está alocado nesta viagem."));

        viagemVan.AlocarMotorista(request.MotoristaId);

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ViagemResponse>(viagem);
        return Result<ViagemResponse>.Success(response);
    }

     public async Task<Result<bool>> RemoverMotoristaAsync(
        Guid gerenteUsuarioId,
        Guid viagemId,
        Guid viagemVanId,
        CancellationToken cancellationToken = default)
    {
        var viagem = await _viagemRepo.GetByIdAsync(viagemId, cancellationToken);
        if (viagem is null)
            return Result<bool>.Failure(Error.NotFound("VIAGEM_NAO_ENCONTRADA", "Viagem não encontrada."));

        if (viagem.GerenteUsuarioId != gerenteUsuarioId)
            return Result<bool>.Failure(Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para alterar esta viagem."));

        if (viagem.Status != StatusViagem.Agendada)
            return Result<bool>.Failure(Error.Validation("STATUS_INVALIDO", "Apenas viagens agendadas podem ter motoristas removidos."));

        var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == viagemVanId);
        if (viagemVan is null)
            return Result<bool>.Failure(Error.NotFound("MOTORISTA_NAO_ALOCADO", "Não existe vínculo entre viagem e van."));
        if (viagemVan.MotoristaUsuarioId == null) return Result<bool>.Failure(Error.Validation("MOTORISTA_NAO_ENCONTRADO", "Esta van já não possui nenhum motorista alocado nesta viagem."));

        

        viagemVan.DesalocarMotorista();

        await _viagemRepo.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    

}
