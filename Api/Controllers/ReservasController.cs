using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Reservas;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarReservaRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId is null)
            return Unauthorized();

        var result = await _reservaService.CriarAsync(usuarioId.Value, request, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return CreatedAtAction(nameof(ObterPorId), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("minhas")]
    [ProducesResponseType(typeof(List<ReservaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Minhas(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId is null)
            return Unauthorized();

        var result = await _reservaService.ListarMinhasAsync(usuarioId.Value, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId is null)
            return Unauthorized();

        var result = await _reservaService.ObterPorIdAsync(usuarioId.Value, id, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/pagar")]
    [ProducesResponseType(typeof(PagarReservaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pagar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId is null)
            return Unauthorized();

        var result = await _reservaService.GerarPagamentoAsync(usuarioId.Value, id, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId is null)
            return Unauthorized();

        var result = await _reservaService.CancelarAsync(usuarioId.Value, id, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private Guid? ObterUsuarioId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
