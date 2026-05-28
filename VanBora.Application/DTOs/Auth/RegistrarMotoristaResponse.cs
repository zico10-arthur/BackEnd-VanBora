namespace VanBora.Application.DTOs.Auth;

public class RegistrarMotoristaResponse
{
    public Guid Id { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public bool Ativo { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime? DataAtualizacao { get; init; }
    public string Cnh { get; init; } = string.Empty;
    public Guid CriadoPorUsuarioId { get; init; }
}