namespace VanBora.Application.DTOs.Auth;

public sealed class AlterarSenhaRequest
{
    public string SenhaAtual { get; init; } = string.Empty;
    public string SenhaNova { get; init; } = string.Empty;
}
