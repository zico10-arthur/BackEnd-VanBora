namespace VanBora.Application.DTOs.Auth;

public class AtualizarUsuarioResponse
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefone { get; init; }
    public string Cpf { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? ChavePix { get; init; }
    public string? NumeroCNH { get; init; }
}
