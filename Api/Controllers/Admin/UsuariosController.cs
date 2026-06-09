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
public class UsuariosController : ControllerBase
{
    private readonly IAdminService _adminService;

    public UsuariosController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BuscarUsuarios(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.BuscarUsuariosAsync(search, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/reservas")]
    [ProducesResponseType(typeof(List<ReservaHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterHistoricoReservas(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ObterHistoricoReservasUsuarioAsync(id, cancellationToken);

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
