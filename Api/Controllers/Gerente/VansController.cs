using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Vans;
using VanBora.Application.Interfaces;

namespace Api.Controllers.Gerente;

/// <summary>
///     Endpoints de gerenciamento de vans (exclusivo para gerentes).
/// </summary>
[ApiController]
[Authorize]
[Route("api/gerente/vans")]
public class VansController : ControllerBase
{
    private readonly IVanService _vanService;

    public VansController(IVanService vanService)
    {
        _vanService = vanService;
    }

    /// <summary>
    ///     Lista todas as vans do gerente autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _vanService.ListarPorGerenteAsync(gerenteId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Obtém uma van específica pelo ID.
    /// </summary>
    [HttpGet("{vanId:guid}")]
    [ProducesResponseType(typeof(VanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid vanId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _vanService.ObterPorIdAsync(gerenteId, vanId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Cadastra uma nova van para o gerente autenticado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarVanRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _vanService.CriarAsync(gerenteId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Atualiza os dados de uma van.
    /// </summary>
    [HttpPut("{vanId:guid}")]
    [ProducesResponseType(typeof(VanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(
        Guid vanId,
        [FromBody] AtualizarVanRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _vanService.AtualizarAsync(gerenteId, vanId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Remove (soft-delete) uma van.
    /// </summary>
    [HttpDelete("{vanId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid vanId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _vanService.RemoverAsync(gerenteId, vanId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private Guid ObterGerenteId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

        return Guid.Parse(sub);
    }
}
