using AutoMapper;
using FluentValidation;
using VanBora.Application.DTOs.Reservas;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;

namespace VanBora.Application.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepo;
    private readonly IViagemVanRepository _viagemVanRepo;
    private readonly IValidator<CriarReservaRequest> _validator;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPagamentoGateway _pagamentoGateway;

    public ReservaService(
        IReservaRepository reservaRepo,
        IViagemVanRepository viagemVanRepo,
        IValidator<CriarReservaRequest> validator,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IPagamentoGateway pagamentoGateway)
    {
        _reservaRepo = reservaRepo;
        _viagemVanRepo = viagemVanRepo;
        _validator = validator;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _pagamentoGateway = pagamentoGateway;
    }

    public async Task<Result<ReservaResponse>> CriarReservaAsync(
        Guid usuarioId,
        CriarReservaRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request com CriarReservaValidator
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<ReservaResponse>.Failure(Error.Validation(erros));
        }

        // 2. Buscar ViagemVan por ID (incluindo Van + Viagem + GerenteUsuario)
        var viagemVan = await _viagemVanRepo.GetByIdAsync(request.ViagemVanId, cancellationToken);

        // 3. Se não encontrada → Error.NotFound
        if (viagemVan is null)
            return Result<ReservaResponse>.Failure(
                Error.NotFound("VIAGEMVAN_NAO_ENCONTRADA", "Van/Viagem não encontrada."));

        var viagem = viagemVan.Viagem;
        var van = viagemVan.Van;

        // 4. Validar que a Viagem está Agendada
        if (viagem.Status != StatusViagem.Agendada)
            return Result<ReservaResponse>.Failure(
                Error.Validation("VIAGEM_NAO_AGENDADA", "A viagem não está disponível para reservas."));

        // 5. Validar que a Van está Ativa
        if (!van.Ativo)
            return Result<ReservaResponse>.Failure(
                Error.Validation("VAN_INATIVA", "A van selecionada não está ativa."));

        // 6. Buscar assentos ocupados (apenas PendentePagamento ou Confirmada com ExpiraEm >= now)
        var assentosOcupados = await _reservaRepo.GetAssentosOcupadosAsync(
            request.ViagemVanId, cancellationToken);

        var assentosOcupadosSet = new HashSet<int>(assentosOcupados);

        // 7. Validar que nenhum assento solicitado está ocupado
        var assentoOcupado = request.Itens
            .Select(i => i.NumeroAssento)
            .FirstOrDefault(a => assentosOcupadosSet.Contains(a));

        if (assentoOcupado != default)
            return Result<ReservaResponse>.Failure(
                Error.Conflict("ASSENTO_OCUPADO", $"O assento {assentoOcupado} já está ocupado."));

        var assentosParaReserva = viagemVan.ObterQuantidadeAssentosParaReserva();

        // 8. Validar que cada NumeroAssento <= Van.Capacidade - 1
        var assentoInvalido = request.Itens
            .Select(i => i.NumeroAssento)
            .FirstOrDefault(a => a > assentosParaReserva);

        if (assentoInvalido != default)
            return Result<ReservaResponse>.Failure(
                Error.Validation("ASSENTO_INVALIDO", $"Assento {assentoInvalido} não existe. Os assentos disponíveis vão de 1 a {assentosParaReserva}."));

        // 9. Validar que não excede capacidade disponível
        var assentosDisponiveis = assentosParaReserva - assentosOcupadosSet.Count;

        if (request.Itens.Count > assentosDisponiveis)
            return Result<ReservaResponse>.Failure(
                Error.Validation("CAPACIDADE_EXCEDIDA", $"Apenas {assentosDisponiveis} assento(s) disponível(is)."));

        // 10. Obter TaxaPlataforma do GerenteUsuario
        var gerente = viagem.GerenteUsuario;
        var taxaPlataformaPercentual = gerente?.TaxaPlataforma ?? 0m;

        // 11. Calcular valorTotal e taxaPlataforma
        var valorTotal = request.Itens.Count * viagem.PrecoAssento;
        var taxaPlataforma = valorTotal * (taxaPlataformaPercentual / 100m);

        // 12. Gerar reservaId manualmente (necessário antes do codigoPix)
        var reservaId = Guid.NewGuid();

        // 13. Gerar codigoPix mock
        var codigoPix = $"pix-mock-{reservaId:N}";

        // 14. Iniciar transação
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 16. Criar entidade Reserva
            var reserva = new Reserva(
                usuarioId,
                request.ViagemVanId,
                valorTotal,
                taxaPlataforma,
                codigoPix);

            // 17. Para cada item do request, criar ItemReserva
            foreach (var item in request.Itens)
            {
                // a. Criar Dinheiro via factory method
                var precoAssentoResult = Dinheiro.Criar(viagem.PrecoAssento);
                if (precoAssentoResult.IsFailure)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ReservaResponse>.Failure(precoAssentoResult.Error!);
                }

                // b. Converter strings para Value Objects
                var emailResult = Email.Criar(item.EmailPassageiro);
                if (emailResult.IsFailure)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ReservaResponse>.Failure(emailResult.Error!);
                }

                var telefoneResult = Telefone.Criar(item.TelefonePassageiro);
                if (telefoneResult.IsFailure)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ReservaResponse>.Failure(telefoneResult.Error!);
                }

                var cpfResult = CPF.Criar(item.CpfPassageiro);
                if (cpfResult.IsFailure)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ReservaResponse>.Failure(cpfResult.Error!);
                }

                // c. Criar ItemReserva
                var itemReserva = new ItemReserva(
                    item.NumeroAssento,
                    precoAssentoResult.Value,
                    item.NomePassageiro,
                    emailResult.Value,
                    telefoneResult.Value,
                    cpfResult.Value);

                // d. Adicionar à reserva
                reserva.AdicionarItem(itemReserva);
            }

            // 18. Salvar
            await _reservaRepo.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 19. Commitar transação
            await _unitOfWork.CommitAsync(cancellationToken);

            // 20. Mapear para ReservaResponse e retornar Success
            var response = _mapper.Map<ReservaResponse>(reserva);
            return Result<ReservaResponse>.Success(response);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<List<ReservaResponse>>> ListarMinhasReservasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var reservas = await _reservaRepo.GetByUsuarioIdAsync(usuarioId, cancellationToken);
        var response = _mapper.Map<List<ReservaResponse>>(reservas);
        return Result<List<ReservaResponse>>.Success(response);
    }

    public async Task<Result<ReservaResponse>> ObterReservaPorIdAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, cancellationToken);

        if (reserva is null)
            return Result<ReservaResponse>.Failure(
                Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada."));

        if (reserva.UsuarioId != usuarioId)
            return Result<ReservaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para visualizar esta reserva."));

        var response = _mapper.Map<ReservaResponse>(reserva);
        return Result<ReservaResponse>.Success(response);
    }

    public async Task<Result<PagarReservaResponse>> PagarReservaAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, cancellationToken);

        if (reserva is null)
            return Result<PagarReservaResponse>.Failure(
                Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada."));

        if (reserva.UsuarioId != usuarioId)
            return Result<PagarReservaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para pagar esta reserva."));

        if (reserva.Status != StatusReserva.PendentePagamento)
            return Result<PagarReservaResponse>.Failure(
                Error.Validation("RESERVA_NAO_PENDENTE", "Esta reserva não está pendente de pagamento."));

        if (reserva.EstaExpirada())
            return Result<PagarReservaResponse>.Failure(
                Error.Validation("RESERVA_EXPIRADA", "O prazo para pagamento desta reserva expirou."));

        var viagem = reserva.ViagemVan.Viagem;
        var titulo = $"VanBora — {viagem.NomeEvento}";

        var preferenciaResult = await _pagamentoGateway.CriarPreferenciaAsync(
            reserva.Id,
            titulo,
            reserva.ValorTotal,
            reserva.ExpiraEm,
            cancellationToken);

        if (preferenciaResult.IsFailure)
            return Result<PagarReservaResponse>.Failure(preferenciaResult.Error!);

        var preferencia = preferenciaResult.Value;

        var response = new PagarReservaResponse
        {
            Id = reserva.Id,
            Status = reserva.Status.ToString(),
            InitPoint = preferencia.InitPoint,
            SandboxInitPoint = preferencia.SandboxInitPoint,
            PreferenceId = preferencia.PreferenceId,
            ValorAPagar = reserva.ValorTotal,
            ExpiraEm = reserva.ExpiraEm
        };

        return Result<PagarReservaResponse>.Success(response);
    }

    public async Task<Result<ReservaResponse>> CancelarReservaAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, cancellationToken);

        if (reserva is null)
            return Result<ReservaResponse>.Failure(
                Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada."));

        if (reserva.UsuarioId != usuarioId)
            return Result<ReservaResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para cancelar esta reserva."));

        if (reserva.Status is StatusReserva.Concluida or StatusReserva.Cancelada)
            return Result<ReservaResponse>.Failure(
                Error.Validation("CANCELAMENTO_INVALIDO", "Esta reserva não pode ser cancelada."));

        reserva.Cancelar();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ReservaResponse>(reserva);
        return Result<ReservaResponse>.Success(response);
    }

    public async Task<Result<ContatoGerenteResponse>> ObterContatoGerenteAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, cancellationToken);

        if (reserva is null)
            return Result<ContatoGerenteResponse>.Failure(
                Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada."));

        if (reserva.UsuarioId != usuarioId)
            return Result<ContatoGerenteResponse>.Failure(
                Error.Forbidden("ACESSO_NEGADO", "Você não tem permissão para visualizar esta reserva."));

        if (reserva.Status != StatusReserva.Confirmada)
            return Result<ContatoGerenteResponse>.Failure(
                Error.Validation("RESERVA_NAO_CONFIRMADA", "O contato do gerente só fica disponível após a confirmação do pagamento."));

        var viagem = reserva.ViagemVan.Viagem;

        return Result<ContatoGerenteResponse>.Success(new ContatoGerenteResponse
        {
            Telefone = viagem.GerenteUsuario.Telefone?.ToString(),
            PossuiIngresso = viagem.PossuiIngresso
        });
    }

    public async Task<Result> ProcessarWebhookPagamentoAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var pagamentoResult = await _pagamentoGateway.ObterPagamentoAsync(paymentId, cancellationToken);
        if (pagamentoResult.IsFailure)
            return Result.Failure(pagamentoResult.Error!);

        var info = pagamentoResult.Value;
        if (info.Status != "approved")
            return Result.Success();

        if (string.IsNullOrWhiteSpace(info.ExternalReference) ||
            !Guid.TryParse(info.ExternalReference, out var reservaId))
            return Error.Validation("RESERVA_ID_INVALIDO", "External reference inválido no pagamento.");

        var reserva = await _reservaRepo.GetByIdAsync(reservaId, cancellationToken);
        if (reserva is null)
            return Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada para o pagamento.");

        reserva.ConfirmarPagamento(info.PaymentId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task ExpirarReservasPendentesAsync(
        CancellationToken cancellationToken = default)
    {
        var reservasExpiradas = await _reservaRepo.GetReservasPendentesExpiradasAsync(cancellationToken);

        foreach (var reserva in reservasExpiradas)
        {
            reserva.ExpiracaoAutomatica();
        }

        if (reservasExpiradas.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
