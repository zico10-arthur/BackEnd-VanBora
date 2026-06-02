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
    private readonly IPagamentoGateway _pagamentoGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CriarReservaRequest> _criarValidator;

    public ReservaService(
        IReservaRepository reservaRepo,
        IViagemVanRepository viagemVanRepo,
        IPagamentoGateway pagamentoGateway,
        IUnitOfWork unitOfWork,
        IValidator<CriarReservaRequest> criarValidator)
    {
        _reservaRepo = reservaRepo;
        _viagemVanRepo = viagemVanRepo;
        _pagamentoGateway = pagamentoGateway;
        _unitOfWork = unitOfWork;
        _criarValidator = criarValidator;
    }

    public async Task<Result<ReservaResponse>> CriarAsync(
        Guid usuarioId,
        CriarReservaRequest request,
        CancellationToken ct = default)
    {
        var validacao = await _criarValidator.ValidateAsync(request, ct);
        if (!validacao.IsValid)
        {
            var msg = string.Join(" ", validacao.Errors.Select(e => e.ErrorMessage));
            return Error.Validation("VALIDACAO_DTO", msg);
        }

        var viagemVan = await _viagemVanRepo.GetByIdAsync(request.ViagemVanId, ct);
        if (viagemVan is null)
            return Error.NotFound("VIAGEM_VAN_NAO_ENCONTRADA", "Viagem/van não encontrada.");

        if (viagemVan.Viagem.Status != StatusViagem.Agendada)
            return Error.Validation("VIAGEM_INDISPONIVEL", "Esta viagem não aceita novas reservas.");

        var assentosOcupados = await _reservaRepo.GetAssentosOcupadosAsync(request.ViagemVanId, ct);
        var maxAssentos = viagemVan.ObterQuantidadeAssentosParaReserva();

        foreach (var item in request.Itens)
        {
            if (item.NumeroAssento > maxAssentos)
                return Error.Validation("ASSENTO_INVALIDO", $"Assento {item.NumeroAssento} não existe nesta van.");

            if (assentosOcupados.Contains(item.NumeroAssento))
                return Error.Conflict("ASSENTO_OCUPADO", $"Assento {item.NumeroAssento} já está ocupado.");
        }

        var duplicados = request.Itens.GroupBy(i => i.NumeroAssento).Where(g => g.Count() > 1).ToList();
        if (duplicados.Count > 0)
            return Error.Validation("ASSENTO_DUPLICADO", "Não é possível reservar o mesmo assento duas vezes.");

        var precoAssento = viagemVan.Viagem.PrecoAssento;
        var valorTotal = precoAssento * request.Itens.Count;
        var gerente = viagemVan.Viagem.GerenteUsuario;
        var taxaPlataforma = gerente.Gratuito == true
            ? 0m
            : Math.Round(valorTotal * (gerente.TaxaPlataforma ?? 0m) / 100m, 2, MidpointRounding.AwayFromZero);

        var reserva = new Reserva(
            usuarioId,
            request.ViagemVanId,
            valorTotal,
            taxaPlataforma,
            Reserva.CodigoPixPendente);

        foreach (var itemReq in request.Itens)
        {
            var email = Email.Criar(itemReq.EmailPassageiro);
            if (email.IsFailure) return email.Error;

            var telefone = Telefone.Criar(itemReq.TelefonePassageiro);
            if (telefone.IsFailure) return telefone.Error;

            var cpf = CPF.Criar(itemReq.CpfPassageiro);
            if (cpf.IsFailure) return cpf.Error;

            var preco = Dinheiro.Criar(precoAssento);
            if (preco.IsFailure) return preco.Error;

            var item = new ItemReserva(
                itemReq.NumeroAssento,
                preco.Value,
                itemReq.NomePassageiro.Trim(),
                email.Value,
                telefone.Value,
                cpf.Value);

            reserva.AdicionarItem(item);
        }

        await _reservaRepo.AddAsync(reserva, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapearResposta(reserva);
    }

    public async Task<Result<PagarReservaResponse>> GerarPagamentoAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken ct = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct);
        if (reserva is null)
            return Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada.");

        if (reserva.UsuarioId != usuarioId)
            return Error.Forbidden("RESERVA_NAO_AUTORIZADA", "Esta reserva não pertence ao usuário.");

        if (reserva.Status != StatusReserva.PendentePagamento)
            return Error.Validation("RESERVA_NAO_PENDENTE", "Reserva já confirmada ou cancelada.");

        if (reserva.EstaExpirada())
        {
            reserva.ExpiracaoAutomatica();
            _reservaRepo.Update(reserva);
            await _unitOfWork.SaveChangesAsync(ct);
            return Error.Validation("RESERVA_EXPIRADA", "Reserva expirada. Crie uma nova reserva.");
        }

        if (reserva.CodigoPix != Reserva.CodigoPixPendente &&
            reserva.CodigoPix.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return Result<PagarReservaResponse>.Success(new PagarReservaResponse
            {
                Id = reserva.Id,
                Status = reserva.Status.ToString(),
                InitPoint = reserva.CodigoPix,
                PreferenceId = reserva.TransacaoId ?? string.Empty,
                ValorAPagar = reserva.ValorAPagar(),
                ExpiraEm = reserva.ExpiraEm
            });
        }

        var titulo = $"VanBora — {reserva.ViagemVan.Viagem.NomeEvento}";
        var preferencia = await _pagamentoGateway.CriarPreferenciaAsync(
            reserva.Id,
            titulo,
            reserva.ValorAPagar(),
            reserva.ExpiraEm,
            ct);

        if (preferencia.IsFailure)
            return preferencia.Error;

        reserva.DefinirLinkPagamento(
            preferencia.Value.InitPoint,
            preferencia.Value.PreferenceId);

        _reservaRepo.Update(reserva);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PagarReservaResponse>.Success(new PagarReservaResponse
        {
            Id = reserva.Id,
            Status = reserva.Status.ToString(),
            InitPoint = preferencia.Value.InitPoint,
            SandboxInitPoint = preferencia.Value.SandboxInitPoint,
            PreferenceId = preferencia.Value.PreferenceId,
            ValorAPagar = reserva.ValorAPagar(),
            ExpiraEm = reserva.ExpiraEm
        });
    }

    public async Task<Result<ReservaResponse>> ObterPorIdAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken ct = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct);
        if (reserva is null)
            return Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada.");

        if (reserva.UsuarioId != usuarioId)
            return Error.Forbidden("RESERVA_NAO_AUTORIZADA", "Esta reserva não pertence ao usuário.");

        return MapearResposta(reserva);
    }

    public async Task<Result<List<ReservaResponse>>> ListarMinhasAsync(
        Guid usuarioId,
        CancellationToken ct = default)
    {
        var reservas = await _reservaRepo.GetByUsuarioIdAsync(usuarioId, ct);
        return reservas.Select(MapearResposta).ToList();
    }

    public async Task<Result<ReservaResponse>> CancelarAsync(
        Guid usuarioId,
        Guid reservaId,
        CancellationToken ct = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct);
        if (reserva is null)
            return Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada.");

        if (reserva.UsuarioId != usuarioId)
            return Error.Forbidden("RESERVA_NAO_AUTORIZADA", "Esta reserva não pertence ao usuário.");

        if (reserva.Status == StatusReserva.Concluida)
            return Error.Validation("RESERVA_CONCLUIDA", "Reserva já finalizada não pode ser cancelada.");

        reserva.Cancelar();
        _reservaRepo.Update(reserva);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapearResposta(reserva);
    }

    public async Task<Result> ProcessarWebhookPagamentoAsync(string paymentId, CancellationToken ct = default)
    {
        var pagamento = await _pagamentoGateway.ObterPagamentoAsync(paymentId, ct);
        if (pagamento.IsFailure)
            return pagamento.Error;

        var info = pagamento.Value;
        if (info.Status is not ("approved" or "paid"))
            return Result.Success();

        if (string.IsNullOrWhiteSpace(info.ExternalReference) ||
            !Guid.TryParse(info.ExternalReference, out var reservaId))
            return Error.Validation("REFERENCIA_INVALIDA", "external_reference inválido no pagamento.");

        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct);
        if (reserva is null)
            return Error.NotFound("RESERVA_NAO_ENCONTRADA", "Reserva não encontrada para o pagamento.");

        if (reserva.Status == StatusReserva.Confirmada)
            return Result.Success();

        if (reserva.Status != StatusReserva.PendentePagamento)
            return Error.Validation("RESERVA_INVALIDA", "Reserva não está pendente de pagamento.");

        reserva.ConfirmarPagamento(info.PaymentId);
        _reservaRepo.Update(reserva);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task ExpirarReservasPendentesAsync(CancellationToken ct = default)
    {
        var expiraveis = await _reservaRepo.GetExpiraveisAsync(ct);
        if (expiraveis.Count == 0)
            return;

        foreach (var reserva in expiraveis)
            reserva.ExpiracaoAutomatica();

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static ReservaResponse MapearResposta(Reserva reserva) =>
        new()
        {
            Id = reserva.Id,
            ViagemVanId = reserva.ViagemVanId,
            Status = reserva.Status.ToString(),
            ValorTotal = reserva.ValorTotal,
            TaxaPlataforma = reserva.TaxaPlataforma,
            ValorAPagar = reserva.ValorAPagar(),
            CodigoPix = reserva.CodigoPix,
            ExpiraEm = reserva.ExpiraEm,
            CriadoEm = reserva.CriadoEm,
            PagoEm = reserva.PagoEm,
            Itens = reserva.Itens.Select(i => new ItemReservaResponse
            {
                Id = i.Id,
                NumeroAssento = i.NumeroAssento,
                PrecoAssento = i.PrecoAssento.Valor,
                NomePassageiro = i.NomePassageiro
            }).ToList()
        };
}
