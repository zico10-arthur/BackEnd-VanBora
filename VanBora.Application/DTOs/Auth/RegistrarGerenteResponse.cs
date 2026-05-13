namespace VanBora.Application.DTOs.Auth;

public class RegistrarGerenteResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public GerenteResponse Gerente { get; set; } = null!;
    public List<string> Perfis { get; set; } = [];
    public string Token { get; set; } = string.Empty;
}
