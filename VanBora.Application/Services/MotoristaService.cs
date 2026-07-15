using AutoMapper;
using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class MotoristaService : IMotoristaService
{
    private readonly IUsuarioRepository _repository;
    private readonly IViagemVanRepository _viagemVanRepository;
    private readonly IValidator<RegistrarMotoristaRequest> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public MotoristaService(
        IUsuarioRepository repository,
        IViagemVanRepository viagemVanRepository,
        IValidator<RegistrarMotoristaRequest> validator,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _repository = repository;
        _viagemVanRepository = viagemVanRepository;
        _validator = validator;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<RegistrarMotoristaResponse>> RegistrarMotorista(
        Guid gerenteId,
        RegistrarMotoristaRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<RegistrarMotoristaResponse>.Failure(Error.Validation(erros));
        }

        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "O usuário criador não foi encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não tem permissão para criar motorista."));

        var cpfResult = CPF.Criar(request.Cpf);
        if (cpfResult.IsFailure)
            return Result<RegistrarMotoristaResponse>.Failure(cpfResult.Error!);

        var cpf = cpfResult.Value;

        var cnhResult = CNH.Criar(request.Cnh);
        if (cnhResult.IsFailure)
            return Result<RegistrarMotoristaResponse>.Failure(cnhResult.Error!);

        var cnh = cnhResult.Value;

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telefoneResult = Telefone.Criar(request.Telefone);
            if (telefoneResult.IsFailure)
                return Result<RegistrarMotoristaResponse>.Failure(telefoneResult.Error!);

            telefone = telefoneResult.Value;
        }

        // Valida se o CPF já existe no sistema
        var usuarioExistente = await _repository.GetByCpfAsync(cpf, ct);
        if (usuarioExistente is not null)
        {
            // Se o CPF já existe e é um Motorista do mesmo gerente → idempotente
            if (usuarioExistente.Tipo == TipoUsuario.Motorista
                && usuarioExistente.CriadoPorUsuarioId == gerenteId)
            {
                // Se o motorista estava inativo, reativa
                if (!usuarioExistente.Ativo)
                {
                    usuarioExistente.Ativar();
                    if (usuarioExistente.CNH is null || usuarioExistente.CNH.Valor != cnh.Valor)
                    {
                        usuarioExistente.RegistrarCNH(cnh);
                    }
                    _repository.Update(usuarioExistente);
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                return Result<RegistrarMotoristaResponse>.Failure(
                    Error.Conflict("CPF_JA_CADASTRADO", "CPF já cadastrado no sistema."));
            }

            // CPF já cadastrado como outro tipo de usuário
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Conflict("CPF_JA_CADASTRADO", "CPF já cadastrado no sistema."));
        }

        // Valida se a CNH já existe
        var motoristas = await _repository.GetMotoristasByGerenteIdAsync(gerenteId, ct);
        if (motoristas.Any(m => m.CNH is not null && m.CNH.Valor == cnh.Valor))
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Conflict("CNH_JA_CADASTRADA", "CNH já cadastrada por este gerente."));

        var motorista = Usuario.CriarMotorista(
            request.Nome,
            cpf,
            telefone,
            cnh,
            gerenteId);

        await _repository.AddAsync(motorista, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await EnviarEmailBoasVindasAsync(motorista, ct);

        var response = _mapper.Map<RegistrarMotoristaResponse>(motorista);
        return Result<RegistrarMotoristaResponse>.Success(response);
    }

    public async Task<Result<List<RegistrarMotoristaResponse>>> ListarMotoristas(
        Guid gerenteId,
        CancellationToken ct = default)
    {
        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<List<RegistrarMotoristaResponse>>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Gerente não encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<List<RegistrarMotoristaResponse>>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não é um gerente."));

        var motoristas = await _repository.GetMotoristasByGerenteIdAsync(gerenteId, ct);
        var response = _mapper.Map<List<RegistrarMotoristaResponse>>(motoristas);
        return Result<List<RegistrarMotoristaResponse>>.Success(response);
    }

    public async Task<Result<RegistrarMotoristaResponse>> ObterMotoristaPorId(
        Guid gerenteId,
        Guid motoristaId,
        CancellationToken ct = default)
    {
        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Gerente não encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não é um gerente."));

        var motorista = await _repository.GetByIdAsync(motoristaId, ct);

        if (motorista is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("MOTORISTA_NAO_ENCONTRADO", "Motorista não encontrado."));

        if (motorista.Tipo != TipoUsuario.Motorista)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Validation("USUARIO_NAO_E_MOTORISTA", "O usuário informado não é um motorista."));

        if (motorista.CriadoPorUsuarioId != gerenteId)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Motorista não pertence a este gerente."));

        var response = _mapper.Map<RegistrarMotoristaResponse>(motorista);
        return Result<RegistrarMotoristaResponse>.Success(response);
    }

    public async Task<Result<RegistrarMotoristaResponse>> AtualizarMotorista(
        Guid gerenteId,
        Guid motoristaId,
        RegistrarMotoristaRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<RegistrarMotoristaResponse>.Failure(Error.Validation(erros));
        }

        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Gerente não encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não é um gerente."));

        var motorista = await _repository.GetByIdAsync(motoristaId, ct);

        if (motorista is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("MOTORISTA_NAO_ENCONTRADO", "Motorista não encontrado."));

        if (motorista.Tipo != TipoUsuario.Motorista)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Validation("USUARIO_NAO_E_MOTORISTA", "O usuário informado não é um motorista."));

        if (motorista.CriadoPorUsuarioId != gerenteId)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Motorista não pertence a este gerente."));

        var cnhResult = CNH.Criar(request.Cnh);
        if (cnhResult.IsFailure)
            return Result<RegistrarMotoristaResponse>.Failure(cnhResult.Error!);

        Telefone? telefone = null;
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telefoneResult = Telefone.Criar(request.Telefone);
            if (telefoneResult.IsFailure)
                return Result<RegistrarMotoristaResponse>.Failure(telefoneResult.Error!);

            telefone = telefoneResult.Value;
        }

        // Valida se a CNH já está em uso por outro motorista do mesmo gerente (só se a CNH foi alterada)
        var cnhAlterada = motorista.CNH is null || motorista.CNH.Valor != cnhResult.Value.Valor;
        if (cnhAlterada)
        {
            var motoristasDoGerente = await _repository.GetMotoristasByGerenteIdAsync(gerenteId, ct);
            if (motoristasDoGerente.Any(m => m.Id != motoristaId && m.CNH is not null && m.CNH.Valor == cnhResult.Value.Valor))
                return Result<RegistrarMotoristaResponse>.Failure(
                    Error.Conflict("CNH_JA_CADASTRADA", "CNH já cadastrada por este gerente."));
        }

        motorista.AtualizarDados(request.Nome, motorista.Email, telefone);
        motorista.RegistrarCNH(cnhResult.Value);

        _repository.Update(motorista);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<RegistrarMotoristaResponse>(motorista);
        return Result<RegistrarMotoristaResponse>.Success(response);
    }

    public async Task<Result<bool>> RemoverMotorista(
        Guid gerenteId,
        Guid motoristaId,
        CancellationToken ct = default)
    {
        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<bool>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Gerente não encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<bool>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não é um gerente."));

        var motorista = await _repository.GetByIdAsync(motoristaId, ct);

        if (motorista is null)
            return Result<bool>.Failure(
                Error.NotFound("MOTORISTA_NAO_ENCONTRADO", "Motorista não encontrado."));

        if (motorista.Tipo != TipoUsuario.Motorista)
            return Result<bool>.Failure(
                Error.Validation("USUARIO_NAO_E_MOTORISTA", "O usuário informado não é um motorista."));

        if (motorista.CriadoPorUsuarioId != gerenteId)
            return Result<bool>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Motorista não pertence a este gerente."));

        // RN12: Soft delete apenas se não estiver alocado em ViagemVan futura
        var alocacoesFuturas = await _viagemVanRepository.GetByMotoristaIdAsync(motoristaId, ct);
        var temAlocacaoFutura = alocacoesFuturas
            .Any(vv => vv.Viagem.DataPartida > DateTime.UtcNow
                       && vv.Viagem.Status != StatusViagem.Cancelada);

        if (temAlocacaoFutura)
            return Result<bool>.Failure(
                Error.Validation("MOTORISTA_COM_VIAGENS_FUTURAS", "Motorista possui viagens futuras. Remova a alocação primeiro."));

        motorista.Desativar();
        _repository.Update(motorista);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<RegistrarMotoristaResponse>> AlternarStatusMotorista(
        Guid gerenteId,
        Guid motoristaId,
        CancellationToken ct = default)
    {
        var gerente = await _repository.GetByIdAsync(gerenteId, ct);

        if (gerente is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("USUARIO_NAO_ENCONTRADO", "Gerente não encontrado."));

        if (gerente.Tipo != TipoUsuario.Gerente)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Unauthorized("USUARIO_NAO_AUTORIZADO", "O usuário não é um gerente."));

        var motorista = await _repository.GetByIdAsync(motoristaId, ct);

        if (motorista is null)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.NotFound("MOTORISTA_NAO_ENCONTRADO", "Motorista não encontrado."));

        if (motorista.Tipo != TipoUsuario.Motorista)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Validation("USUARIO_NAO_E_MOTORISTA", "O usuário informado não é um motorista."));

        if (motorista.CriadoPorUsuarioId != gerenteId)
            return Result<RegistrarMotoristaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Motorista não pertence a este gerente."));

        if (motorista.Ativo)
            motorista.Desativar();
        else
            motorista.Ativar();

        _repository.Update(motorista);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<RegistrarMotoristaResponse>(motorista);
        return Result<RegistrarMotoristaResponse>.Success(response);
    }

    private async Task EnviarEmailBoasVindasAsync(Usuario motorista, CancellationToken ct)
    {
        if (motorista.Email is null) return;

        await _emailService.SendAsync(
            motorista.Email.Valor,
            "Bem-vindo(a) ao VanBora!",
            $"Olá {motorista.Nome}, sua conta de motorista no VanBora foi criada com sucesso! " +
            "Agora você pode acessar seu painel e visualizar as viagens alocadas a você. " +
            "Acesse: http://localhost:3000/entrar",
            cancellationToken: ct);
    }
}