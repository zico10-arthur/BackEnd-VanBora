namespace VanBora.Application.DTOs.Admin;

public class GerenteAdminResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? Slug { get; init; }
    public decimal? TaxaPlataforma { get; init; }
    public bool? Gratuito { get; init; }
    public bool Ativo { get; init; }
    public int TotalVans { get; set; }
    public int TotalViagens { get; set; }
    public DateTime CriadoEm { get; init; }
}
