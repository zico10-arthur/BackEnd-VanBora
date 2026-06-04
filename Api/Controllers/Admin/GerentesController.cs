using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Admin;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Enums;

namespace Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/[controller]")]
public class GerentesController : ControllerBase
{
    private readonly IAdminService _adminService;

    public GerentesController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GerenteAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarGerentes(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ListarGerentesAsync(search, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterGerentePorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ObterGerentePorIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CriarGerente(
        [FromBody] CriarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.CriarGerenteAsync(request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarGerente(
        Guid id,
        [FromBody] AtualizarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.AtualizarGerenteAsync(id, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/reservas")]
    [ProducesResponseType(typeof(List<ViagemGerenteHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterHistoricoReservasGerente(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ObterHistoricoReservasGerenteAsync(id, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private IActionResult? ValidarAdmin()
    {
        var tipoClaim = User.FindFirst("tipos")?.Value;
        if (tipoClaim != TipoUsuario.Admin.ToString())
            return new ObjectResult(Result<object>.Failure(Error.Forbidden("ACESSO_NEGADO", "Acesso restrito a administradores.")));
        return null;
    }
}
