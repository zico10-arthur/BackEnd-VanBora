using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;

namespace Api.Controllers.Gerente;

/// <summary>
///     Endpoints de gerenciamento de motoristas (exclusivo para gerentes).
/// </summary>
[ApiController]
[Authorize]
[Route("api/gerente/motoristas")]
public class MotoristasController : ControllerBase
{
    private readonly IMotoristaService _motoristaService;

    public MotoristasController(IMotoristaService motoristaService)
    {
        _motoristaService = motoristaService;
    }

    /// <summary>
    ///     Lista todos os motoristas do gerente autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RegistrarMotoristaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.ListarMotoristas(gerenteId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Obtém um motorista específico pelo ID.
    /// </summary>
    [HttpGet("{motoristaId:guid}")]
    [ProducesResponseType(typeof(RegistrarMotoristaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid motoristaId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.ObterMotoristaPorId(gerenteId, motoristaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Cadastra um novo motorista para o gerente autenticado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarMotoristaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] RegistrarMotoristaRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.RegistrarMotorista(gerenteId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Atualiza os dados de um motorista.
    /// </summary>
    [HttpPut("{motoristaId:guid}")]
    [ProducesResponseType(typeof(RegistrarMotoristaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(
        Guid motoristaId,
        [FromBody] RegistrarMotoristaRequest request,
        CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.AtualizarMotorista(gerenteId, motoristaId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Remove (soft-delete) um motorista.
    /// </summary>
    [HttpDelete("{motoristaId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid motoristaId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.RemoverMotorista(gerenteId, motoristaId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Alterna o status ativo/inativo de um motorista.
    /// </summary>
    [HttpPatch("{motoristaId:guid}/status")]
    [ProducesResponseType(typeof(RegistrarMotoristaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatus(Guid motoristaId, CancellationToken cancellationToken)
    {
        var gerenteId = ObterGerenteId();
        var result = await _motoristaService.AlternarStatusMotorista(gerenteId, motoristaId, cancellationToken);

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
