using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
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
    ///     ou o status HTTP correspondente ao erro (400, 409, etc.) via ResultFilter.
    /// </returns>
    [HttpPost("gerente/registrar")]
    [ProducesResponseType(typeof(RegistrarGerenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarGerente(
        [FromBody] RegistrarGerenteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegistrarGerente(request, cancellationToken);

        if (result.IsFailure)
        {
            // Devolve um ObjectResult com o Result<T> (IAppResult)
            // para que o ResultFilter capture e converta ao status HTTP adequado
            return new ObjectResult(result);
        }

        // 201 Created com o corpo da resposta
        return Created(string.Empty, result.Value);
    }

    /// <summary>
    ///     Realiza o login do usuário (Passageiro ou Gerente).
    /// </summary>
    /// <param name="request">Credenciais de login (email e senha).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com os dados do usuário e token JWT em caso de sucesso,
    ///     ou o status HTTP correspondente ao erro (401, 403, etc.) via ResultFilter.
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
        {
            return new ObjectResult(result);
        }

        return Ok(result.Value);
    }
}
