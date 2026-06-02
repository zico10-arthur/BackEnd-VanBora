using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;

namespace Api.Controllers.Gerente;

/// <summary>
///     Endpoints de gerenciamento de viagens (exclusivo para gerentes).
/// </summary>
[ApiController]
[Authorize]
[Route("api/gerente/viagens")]
public class ViagensController : ControllerBase
{
    private readonly IViagemService _viagemService;
    private readonly IRelatorioService _relatorioService;

    public ViagensController(
        IViagemService viagemService,
        IRelatorioService relatorioService)
    {
        _viagemService = viagemService;
        _relatorioService = relatorioService;
    }

    /// <summary>
    ///     Cadastra uma nova viagem.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ViagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarViagemRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.CriarAsync(gerenteId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Lista todas as viagens do gerente autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ViagemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorGerente(CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.ListarPorGerenteAsync(gerenteId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Atualiza os dados de uma viagem.
    /// </summary>
    [HttpPut("{viagemId:guid}")]
    [ProducesResponseType(typeof(ViagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid viagemId,
        [FromBody] AtualizarViagemRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.AtualizarAsync(gerenteId, viagemId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Cancela uma viagem.
    /// </summary>
    [HttpPost("{viagemId:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid viagemId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.CancelarAsync(gerenteId, viagemId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Aloca uma van a uma viagem.
    /// </summary>
    [HttpPost("{viagemId:guid}/alocar-van")]
    [ProducesResponseType(typeof(ViagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlocarVan(
        Guid viagemId,
        [FromBody] AlocarVanRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.AlocarVanAsync(gerenteId, viagemId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Remove a alocação de uma van de uma viagem.
    /// </summary>
    [HttpDelete("{viagemId:guid}/alocar-van/{viagemVanId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverVan(Guid viagemId, Guid viagemVanId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.RemoverVanAsync(gerenteId, viagemId, viagemVanId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Gera relatório financeiro da viagem com indicadores de reservas.
    /// </summary>
    [HttpGet("{viagemId:guid}/relatorio")]
    [ProducesResponseType(typeof(RelatorioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GerarRelatorio(Guid viagemId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _relatorioService.GerarRelatorioAsync(gerenteId, viagemId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Aloca um motorista a uma van da viagem.
    /// </summary>
    [HttpPost("{viagemId:guid}/alocar-motorista/{viagemVanId:guid}")]
    [ProducesResponseType(typeof(ViagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlocarMotorista(
        Guid viagemId,
        Guid viagemVanId,
        [FromBody] AlocarMotoristaRequest request,
        CancellationToken cancellationToken = default)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.AlocarMotoristaAsync(gerenteId, viagemId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Remove a alocação de um motorista de uma van da viagem.
    /// </summary>
    [HttpDelete("{viagemId:guid}/remover-motorista/{viagemVanId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverMotorista(
        Guid viagemId,
        Guid viagemVanId,
        CancellationToken cancellationToken = default)
    {
        var gerenteId = ObterGerenteId();
        var result = await _viagemService.RemoverMotoristaAsync(gerenteId, viagemId, viagemVanId, cancellationToken);

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
