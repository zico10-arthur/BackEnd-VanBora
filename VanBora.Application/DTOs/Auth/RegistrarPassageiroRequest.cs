namespace VanBora.Application.DTOs.Auth;

public sealed class RegistrarPassageiroRequest
{
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
