namespace VanBora.Application.DTOs.Auth;

public class LoginResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Perfis { get; set; } = [];
    public string Token { get; set; } = string.Empty;
}
