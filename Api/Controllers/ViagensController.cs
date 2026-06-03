using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ViagensController : ControllerBase
{
    private readonly IViagemPublicService _viagemPublicService;

    public ViagensController(IViagemPublicService viagemPublicService)
    {
        _viagemPublicService = viagemPublicService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _viagemPublicService.ListarDisponiveisAsync(cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("van/{viagemVanId:guid}")]
    public async Task<IActionResult> DetalheVan(Guid viagemVanId, CancellationToken cancellationToken)
    {
        var result = await _viagemPublicService.ObterDetalheViagemVanAsync(viagemVanId, cancellationToken);
        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }
}
