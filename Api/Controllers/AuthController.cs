using System.Security.Claims;
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
    private readonly IMotoristaService _motorista;

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
    ///     Atualiza os dados do perfil do usuário autenticado (US18).
    /// </summary>
    /// <param name="request">Dados a serem atualizados (nome, email, telefone, chavePix, CNH).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com os dados atualizados,
    ///     ou o status HTTP correspondente ao erro via <see cref="Middleware.ResultFilter" />.
    /// </returns>
    [HttpPut("usuario")]
    [Authorize]
    [ProducesResponseType(typeof(AtualizarUsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarUsuario(
        [FromBody] AtualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _authService.AtualizarUsuarioAsync(usuarioId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Altera a senha do usuário autenticado (US21).
    /// </summary>
    /// <param name="request">Senha atual e nova senha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com mensagem de sucesso,
    ///     ou 401 se a senha atual estiver incorreta,
    ///     ou 404 se o usuário não for encontrado.
    /// </returns>
    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSenha(
        [FromBody] AlterarSenhaRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _authService.AlterarSenhaAsync(usuarioId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(new { mensagem = result.Value });
    }

    /// <summary>
    ///     Atualiza o slug do gerente autenticado (US19).
    /// </summary>
    /// <param name="request">Novo slug.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com os dados atualizados do usuário,
    ///     ou 403 se não for gerente,
    ///     ou 409 se o slug já estiver em uso.
    /// </returns>
    [HttpPut("slug")]
    [Authorize]
    [ProducesResponseType(typeof(AtualizarUsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AtualizarSlug(
        [FromBody] AtualizarSlugRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _authService.AtualizarSlugAsync(usuarioId, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Solicita a exclusão da conta do usuário autenticado (US20).
    ///     Um código de 6 dígitos é enviado para o email cadastrado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com mensagem de sucesso,
    ///     ou 404 se o usuário não for encontrado,
    ///     ou 409 se a conta já estiver desativada ou houver reservas ativas.
    /// </returns>
    [HttpPost("solicitar-exclusao")]
    [Authorize]
    [ProducesResponseType(typeof(SolicitarExclusaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SolicitarExclusao(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _authService.SolicitarExclusaoAsync(usuarioId, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    ///     Confirma a exclusão da conta com o código recebido por email (US20).
    /// </summary>
    /// <param name="request">Código de 6 dígitos enviado por email.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    ///     200 OK com mensagem de sucesso (conta desativada),
    ///     ou 400 se o código for inválido/expirado,
    ///     ou 404 se o usuário não for encontrado.
    /// </returns>
    [HttpPost("confirmar-exclusao")]
    [Authorize]
    [ProducesResponseType(typeof(ConfirmarExclusaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmarExclusao(
        [FromBody] ConfirmarExclusaoRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var result = await _authService.ConfirmarExclusaoAsync(usuarioId, request, cancellationToken);

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

    private Guid ObterUsuarioId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

        return Guid.Parse(sub);
    }
    [HttpPost("motorista/registrar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrarGerenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarMotorista(
        [FromBody] RegistrarMotoristaRequest request,Guid gerenteid,
        CancellationToken ct)
    {
        var result = await _motorista.RegistrarMotorista(gerenteid, request, ct);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }
}
