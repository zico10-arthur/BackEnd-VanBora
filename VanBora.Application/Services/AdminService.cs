using AutoMapper;
using FluentValidation;
using VanBora.Application.DTOs.Admin;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;

namespace VanBora.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IVanRepository _vanRepo;
    private readonly IViagemRepository _viagemRepo;
    private readonly IReservaRepository _reservaRepo;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AtualizarGerenteAdminRequest> _atualizarGerenteValidator;
    private readonly IValidator<CriarGerenteAdminRequest> _criarGerenteValidator;

    public AdminService(
        IUsuarioRepository usuarioRepo,
        IVanRepository vanRepo,
        IViagemRepository viagemRepo,
        IReservaRepository reservaRepo,
        IAuthService authService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IValidator<AtualizarGerenteAdminRequest> atualizarGerenteValidator,
        IValidator<CriarGerenteAdminRequest> criarGerenteValidator)
    {
        _usuarioRepo = usuarioRepo;
        _vanRepo = vanRepo;
        _viagemRepo = viagemRepo;
        _reservaRepo = reservaRepo;
        _authService = authService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _atualizarGerenteValidator = atualizarGerenteValidator;
        _criarGerenteValidator = criarGerenteValidator;
    }

    public async Task<Result<List<GerenteAdminResponse>>> ListarGerentesAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var gerentes = await _usuarioRepo.GetGerentesAsync(search, cancellationToken);

        var response = new List<GerenteAdminResponse>();
        foreach (var gerente in gerentes)
        {
            var dto = await MapearGerenteComContagensAsync(gerente, cancellationToken);
            response.Add(dto);
        }

        return Result<List<GerenteAdminResponse>>.Success(response);
    }

    public async Task<Result<GerenteAdminResponse>> ObterGerentePorIdAsync(
        Guid gerenteId,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(gerenteId, cancellationToken);

        if (usuario is null || usuario.Tipo != TipoUsuario.Gerente)
            return Result<GerenteAdminResponse>.Failure(
                Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente não encontrado."));

        var dto = await MapearGerenteComContagensAsync(usuario, cancellationToken);
        return Result<GerenteAdminResponse>.Success(dto);
    }

    public async Task<Result<GerenteAdminResponse>> CriarGerenteAsync(
        CriarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _criarGerenteValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<GerenteAdminResponse>.Failure(Error.Validation(erros));
        }

        var registrarRequest = new RegistrarGerenteRequest
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Senha = request.Senha,
            Telefone = request.Telefone,
            Slug = request.Slug,
            ChavePix = request.ChavePix
        };

        var registrarResult = await _authService.RegistrarGerente(registrarRequest, cancellationToken);

        if (registrarResult.IsFailure)
            return Result<GerenteAdminResponse>.Failure(registrarResult.Error);

        var usuarioId = registrarResult.Value.UsuarioId;
        var hasCustomParams = request.TaxaPlataforma.HasValue || request.Gratuito.HasValue;

        if (hasCustomParams)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, cancellationToken);
            if (usuario is null)
                return Result<GerenteAdminResponse>.Failure(
                    Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente recém-criado não encontrado."));

            usuario.AtualizarParametrosGerente(request.TaxaPlataforma, request.Gratuito);
            _usuarioRepo.Update(usuario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = await MapearGerenteComContagensAsync(usuario, cancellationToken);
            return Result<GerenteAdminResponse>.Success(dto);
        }

        var gerenteCriado = await _usuarioRepo.GetByIdAsync(usuarioId, cancellationToken);
        if (gerenteCriado is null)
            return Result<GerenteAdminResponse>.Failure(
                Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente recém-criado não encontrado."));

        var response = await MapearGerenteComContagensAsync(gerenteCriado, cancellationToken);
        return Result<GerenteAdminResponse>.Success(response);
    }

    public async Task<Result<GerenteAdminResponse>> AtualizarGerenteAsync(
        Guid gerenteId,
        AtualizarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _atualizarGerenteValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<GerenteAdminResponse>.Failure(Error.Validation(erros));
        }

        var usuario = await _usuarioRepo.GetByIdAsync(gerenteId, cancellationToken);

        if (usuario is null || usuario.Tipo != TipoUsuario.Gerente)
            return Result<GerenteAdminResponse>.Failure(
                Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente não encontrado."));

        var isNop = request.TaxaPlataforma is null
                    && request.Gratuito is null
                    && request.Ativo is null;

        if (isNop)
        {
            var dto = await MapearGerenteComContagensAsync(usuario, cancellationToken);
            return Result<GerenteAdminResponse>.Success(dto);
        }

        usuario.AtualizarParametrosGerente(request.TaxaPlataforma, request.Gratuito);

        if (request.Ativo.HasValue)
        {
            if (request.Ativo.Value)
                usuario.Ativar();
            else
                usuario.Desativar();
        }

        _usuarioRepo.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await MapearGerenteComContagensAsync(usuario, cancellationToken);
        return Result<GerenteAdminResponse>.Success(response);
    }

    public async Task<Result<List<UsuarioAdminResponse>>> BuscarUsuariosAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepo.SearchAllAsync(search, cancellationToken);

        var response = new List<UsuarioAdminResponse>();
        foreach (var usuario in usuarios)
        {
            var totalReservas = await _reservaRepo.GetCountByUsuarioIdAsync(usuario.Id, cancellationToken);
            var dto = _mapper.Map<UsuarioAdminResponse>(usuario);
            dto.TotalReservas = totalReservas;
            response.Add(dto);
        }

        return Result<List<UsuarioAdminResponse>>.Success(response);
    }

    public async Task<Result<List<ReservaHistoricoResponse>>> ObterHistoricoReservasUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result<List<ReservaHistoricoResponse>>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado."));

        var reservas = await _reservaRepo.GetByUsuarioIdAsync(usuarioId, cancellationToken);
        var response = _mapper.Map<List<ReservaHistoricoResponse>>(reservas);
        return Result<List<ReservaHistoricoResponse>>.Success(response);
    }

    public async Task<Result<List<ViagemGerenteHistoricoResponse>>> ObterHistoricoReservasGerenteAsync(
        Guid gerenteId,
        CancellationToken cancellationToken)
    {
        var gerente = await _usuarioRepo.GetByIdAsync(gerenteId, cancellationToken);
        if (gerente is null || gerente.Tipo != TipoUsuario.Gerente)
            return Result<List<ViagemGerenteHistoricoResponse>>.Failure(
                Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente não encontrado."));

        var viagens = await _viagemRepo.GetByGerenteUsuarioIdAsync(gerenteId, cancellationToken);

        var response = new List<ViagemGerenteHistoricoResponse>();
        foreach (var viagem in viagens)
        {
            var reservas = await _reservaRepo.GetByViagemIdAsync(viagem.Id, cancellationToken);
            var reservasConfirmadas = reservas.Where(r => r.Status == StatusReserva.Confirmada).ToList();

            response.Add(new ViagemGerenteHistoricoResponse
            {
                ViagemId = viagem.Id,
                NomeEvento = viagem.NomeEvento,
                Origem = viagem.LocalPartida,
                Destino = viagem.LocalEvento,
                DataPartida = viagem.DataPartida,
                DataEvento = viagem.DataEvento,
                TotalReservas = reservas.Count,
                TotalArrecadado = reservasConfirmadas.Sum(r => r.ValorTotal),
                TaxaPlataforma = reservasConfirmadas.Sum(r => r.TaxaPlataforma),
                StatusViagem = viagem.Status.ToString()
            });
        }

        return Result<List<ViagemGerenteHistoricoResponse>>.Success(response);
    }

    private async Task<GerenteAdminResponse> MapearGerenteComContagensAsync(
        Usuario gerente,
        CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<GerenteAdminResponse>(gerente);

        var vans = await _vanRepo.GetByGerenteUsuarioIdAsync(gerente.Id, cancellationToken);
        var viagens = await _viagemRepo.GetByGerenteUsuarioIdAsync(gerente.Id, cancellationToken);

        dto.TotalVans = vans.Count;
        dto.TotalViagens = viagens.Count;

        return dto;
    }
}
