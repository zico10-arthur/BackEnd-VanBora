namespace VanBora.Application.DTOs.Auth;

public class RegistrarMotoristaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Cnh { get; set; } = string.Empty;
}