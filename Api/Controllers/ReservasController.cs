using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Reservas;
using VanBora.Application.Interfaces;

namespace Api.Controllers;

/// <summary>
///     Endpoints de reservas.
///     Qualquer usuário autenticado pode criar e visualizar suas próprias reservas.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    ///     Cria uma nova reserva de assentos em uma van alocada em uma viagem.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarReserva(
        [FromBody] CriarReservaRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.CriarReservaAsync(usuarioId, request, cancellationToken);

        // ResultFilter intercepta ObjectResult cujo Value implemente IAppResult
        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Lista todas as reservas do usuário autenticado.
    /// </summary>
    [HttpGet("minhas")]
    [ProducesResponseType(typeof(List<ReservaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMinhasReservas(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.ListarMinhasReservasAsync(usuarioId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Obtém os detalhes de uma reserva específica (apenas se for do usuário).
    /// </summary>
    [HttpGet("{reservaId:guid}")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterReservaPorId(
        Guid reservaId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.ObterReservaPorIdAsync(usuarioId, reservaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Gera (ou renova) a preferência de pagamento (Checkout Pix) da reserva no Mercado Pago.
    /// </summary>
    [HttpPost("{reservaId:guid}/pagar")]
    [ProducesResponseType(typeof(PagarReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PagarReserva(
        Guid reservaId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.PagarReservaAsync(usuarioId, reservaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Cancela uma reserva do usuário autenticado.
    /// </summary>
    [HttpPost("{reservaId:guid}/cancelar")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelarReserva(
        Guid reservaId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.CancelarReservaAsync(usuarioId, reservaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Obtém o contato do gerente para reservas de viagens que possuem ingresso (PossuiIngresso = true).
    ///     Retorna o telefone do gerente para que o passageiro possa combinar a compra do ingresso diretamente.
    /// </summary>
    [HttpGet("{reservaId:guid}/contato-gerente")]
    [ProducesResponseType(typeof(ContatoGerenteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterContatoGerente(
        Guid reservaId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _reservaService.ObterContatoGerenteAsync(usuarioId, reservaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private Guid ObterUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
