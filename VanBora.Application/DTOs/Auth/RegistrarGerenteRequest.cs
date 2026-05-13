namespace VanBora.Application.DTOs.Auth;

public class RegistrarGerenteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Slug { get; set; } = string.Empty;
}
