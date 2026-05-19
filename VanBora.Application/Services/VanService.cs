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

        var placaResult = Placa.Criar(request.Placa);
        if (!placaResult.IsSuccess)
            return Result<VanResponse>.Failure(placaResult.Error);

        van.AtualizarDados(request.Nome, placaResult.Value, request.Modelo);

        _vanRepository.Update(van);
        await _vanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<VanResponse>(van);
        return Result<VanResponse>.Success(response);
    }

    public Task<Result<VanResponse>> ObterPorIdAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<VanResponse>>> ListarPorGerenteAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> RemoverAsync(Guid gerenteUsuarioId, Guid vanId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
