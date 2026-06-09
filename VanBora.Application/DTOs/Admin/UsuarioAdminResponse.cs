namespace VanBora.Application.DTOs.Admin;

public class UsuarioAdminResponse
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public int TotalReservas { get; set; }
    public DateTime CriadoEm { get; init; }
}
