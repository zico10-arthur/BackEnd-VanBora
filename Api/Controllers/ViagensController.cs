using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;

namespace Api.Controllers;

/// <summary>
///     Endpoints públicos de consulta de viagens.
///     Qualquer usuário (autenticado ou não) pode visualizar viagens disponíveis.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ViagensController : ControllerBase
{
    private readonly IViagemService _viagemService;
    private readonly IViagemPublicService _viagemPublicService;

    public ViagensController(IViagemService viagemService, IViagemPublicService viagemPublicService)
    {
        _viagemService = viagemService;
        _viagemPublicService = viagemPublicService;
    }

    /// <summary>
    ///     Lista todas as viagens disponíveis (status = Agendada), com vans alocadas e assentos disponíveis.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ViagemPublicaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDisponiveis(CancellationToken cancellationToken)
    {
        var result = await _viagemPublicService.ListarDisponiveisAsync(cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Obtém os detalhes de uma viagem específica, incluindo as vans alocadas.
    /// </summary>
    [HttpGet("{viagemId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ViagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid viagemId, CancellationToken cancellationToken)
    {
        var result = await _viagemService.ObterPorIdAsync(viagemId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Lista as vans alocadas em uma viagem com a quantidade de assentos disponíveis.
    /// </summary>
    [HttpGet("{viagemId:guid}/vans")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ViagemVanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarVans(Guid viagemId, CancellationToken cancellationToken)
    {
        var result = await _viagemService.ObterPorIdAsync(viagemId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value.Vans);
    }

    /// <summary>
    ///     Obtém os detalhes de uma viagem/van específica (evento, van e assentos ocupados),
    ///     usado na tela de reserva de assentos.
    /// </summary>
    [HttpGet("van/{viagemVanId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ViagemVanDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDetalheViagemVan(Guid viagemVanId, CancellationToken cancellationToken)
    {
        var result = await _viagemPublicService.ObterDetalheViagemVanAsync(viagemVanId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }
}
