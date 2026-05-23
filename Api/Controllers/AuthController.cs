using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;

namespace Api.Controllers;

/// <summary>
///     Endpoints de autenticação e cadastro (gerente, passageiro e login).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    ///     Registra um novo gerente na plataforma.
    /// </summary>
    /// <param name="request">Dados de cadastro do gerente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     201 Created com os dados do gerente e token JWT em caso de sucesso,
    ///     ou o status HTTP correspondente ao erro via <see cref="Middleware.ResultFilter" />.
    /// </returns>
    [HttpPost("gerente/registrar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarGerenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarGerente(
        [FromBody] RegistrarGerenteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegistrarGerente(request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Registra um passageiro (novo usuário ou conclusão de conta pendente sem senha, ex.: motorista).
    /// </summary>
    /// <param name="request">Nome, CPF, email, telefone e senha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     201 Created com identificadores e JWT; 400 para erros de validação; 409 para conflitos
    ///     (CPF já cadastrado com senha, email duplicado, etc.).
    /// </returns>
    [HttpPost("passageiro/registrar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarPassageiroResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarPassageiro(
        [FromBody] RegistrarPassageiroRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegistrarPassageiroAsync(request, cancellationToken);

        if (result.IsSuccess)
            return Created(string.Empty, result.Value);

        return MapearFalhaRegistrarPassageiro(result.Error);
    }

    /// <summary>
    ///     Realiza o login do usuário (passageiro ou gerente).
    /// </summary>
    /// <param name="request">Credenciais (email e senha).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com dados e JWT, ou o status HTTP adequado via <see cref="Middleware.ResultFilter" />.
    /// </returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.Login(request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Converte <see cref="Error" /> do cadastro de passageiro em resposta HTTP explícita (US03 / Dev 4).
    /// </summary>
    private IActionResult MapearFalhaRegistrarPassageiro(Error error)
    {
        var corpo = new ErrorResponse(error.Code, error.Message);

        return error.Type switch
        {
            ErrorType.Validation => BadRequest(corpo),
            ErrorType.Conflict => Conflict(corpo),
            _ => StatusCode(StatusCodes.Status500InternalServerError, corpo)
        };
    }

    private sealed record ErrorResponse(string Code, string Message);
}
