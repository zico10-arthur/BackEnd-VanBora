using AutoMapper;
using FluentValidation;
using VanBora.Application.DTOs.Vans;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class VanService : IVanService
{
    private readonly IVanRepository _vanRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CriarVanRequest> _criarValidator;
    private readonly IValidator<AtualizarVanRequest> _atualizarValidator;

    public VanService(
        IVanRepository vanRepository,
        IMapper mapper,
        IValidator<CriarVanRequest> criarValidator,
        IValidator<AtualizarVanRequest> atualizarValidator)
    {
        _vanRepository = vanRepository;
        _mapper = mapper;
        _criarValidator = criarValidator;
        _atualizarValidator = atualizarValidator;
    }

    public async Task<Result<VanResponse>> CriarAsync(Guid gerenteUsuarioId, CriarVanRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _criarValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<VanResponse>.Failure(Error.Validation(erros));
        }

        var placaResult = Placa.Criar(request.Placa);
        if (!placaResult.IsSuccess)
            return Result<VanResponse>.Failure(placaResult.Error);

        // Verifica unicidade da placa
        var vanComMesmaPlaca = await _vanRepository.GetByPlacaAsync(placaResult.Value, cancellationToken);
        if (vanComMesmaPlaca is not null)
            return Result<VanResponse>.Failure(
                Error.Conflict("PLACA_EM_USO", "Esta placa já está cadastrada."));

        // Verifica unicidade do nome para este gerente
        var vansDoGerente = await _vanRepository.GetByGerenteUsuarioIdAsync(gerenteUsuarioId, cancellationToken);
        if (vansDoGerente.Any(v => string.Equals(v.Nome, request.Nome, StringComparison.OrdinalIgnoreCase)))
            return Result<VanResponse>.Failure(
                Error.Conflict("NOME_EM_USO", "Você já tem uma van com este nome."));

        var van = new Van(gerenteUsuarioId, request.Nome, placaResult.Value, request.Modelo, request.Capacidade);

        await _vanRepository.AddAsync(van, cancellationToken);
        await _vanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<VanResponse>(van);
        return Result<VanResponse>.Success(response);
    }

    public async Task<Result<VanResponse>> AtualizarAsync(Guid gerenteUsuarioId, Guid vanId, AtualizarVanRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _atualizarValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<VanResponse>.Failure(Error.Validation(erros));
        }

        var van = await _vanRepository.GetByIdAndGerenteAsync(vanId, gerenteUsuarioId, cancellationToken);
        if (van is null)
            return Result<VanResponse>.Failure(Error.NotFound("VAN_NAO_ENCONTRADA", "Van não encontrada."));

        // Só valida unicidade se a placa foi alterada (evita query desnecessária)
        var placaMudou = !string.Equals(request.Placa, van.Placa.Valor, StringComparison.OrdinalIgnoreCase);

        var novaPlaca = van.Placa; // mantém a placa atual por padrão

        if (placaMudou)
        {
            var placaResult = Placa.Criar(request.Placa);
            if (!placaResult.IsSuccess)
                return Result<VanResponse>.Failure(placaResult.Error);

            novaPlaca = placaResult.Value;

            var vanComMesmaPlaca = await _vanRepository.GetByPlacaAsync(novaPlaca, cancellationToken);
            if (vanComMesmaPlaca is not null)
                return Result<VanResponse>.Failure(
                    Error.Conflict("PLACA_EM_USO", "Esta placa já está cadastrada para outra van."));
        }

        // Verifica unicidade do nome se foi alterado
        var nomeMudou = !string.Equals(request.Nome, van.Nome, StringComparison.OrdinalIgnoreCase);
        if (nomeMudou)
        {
            var vansDoGerente = await _vanRepository.GetByGerenteUsuarioIdAsync(gerenteUsuarioId, cancellationToken);
            if (vansDoGerente.Any(v => v.Id != vanId && string.Equals(v.Nome, request.Nome, StringComparison.OrdinalIgnoreCase)))
                return Result<VanResponse>.Failure(
                    Error.Conflict("NOME_EM_USO", "Você já tem uma van com este nome."));
        }

        van.AtualizarDados(request.Nome, novaPlaca);

        _vanRepository.Update(van);
        await _vanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<VanResponse>(van);
        return Result<VanResponse>.Success(response);
    }

    public async Task<Result<VanResponse>> ObterPorIdAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default)
    {
        var van = await _vanRepository.GetByIdAndGerenteAsync(vanId, gerenteUsuarioId, cancellationToken);
        if (van is null)
            return Result<VanResponse>.Failure(Error.NotFound("VAN_NAO_ENCONTRADA", "Van não encontrada."));

        var response = _mapper.Map<VanResponse>(van);
        return Result<VanResponse>.Success(response);
    }

    public async Task<Result<List<VanResponse>>> ListarPorGerenteAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default)
    {
        var vans = await _vanRepository.GetByGerenteUsuarioIdAsync(gerenteUsuarioId, cancellationToken);

        var response = _mapper.Map<List<VanResponse>>(vans);
        return Result<List<VanResponse>>.Success(response);
    }

    public async Task<Result<bool>> RemoverAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default)
    {
        var van = await _vanRepository.GetByIdAndGerenteAsync(vanId, gerenteUsuarioId, cancellationToken);
        if (van is null)
            return Result<bool>.Failure(Error.NotFound("VAN_NAO_ENCONTRADA", "Van não encontrada."));

        van.Desativar();

        _vanRepository.Update(van);
        await _vanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<VanResponse>> AlternarStatusAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default)
    {
        var van = await _vanRepository.GetByIdAndGerenteAsync(vanId, gerenteUsuarioId, cancellationToken);
        if (van is null)
            return Result<VanResponse>.Failure(Error.NotFound("VAN_NAO_ENCONTRADA", "Van não encontrada."));

        if (van.Ativo)
            van.Desativar();
        else
            van.Ativar();

        _vanRepository.Update(van);
        await _vanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<VanResponse>(van);
        return Result<VanResponse>.Success(response);
    }
}
