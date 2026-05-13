namespace VanBora.Application.DTOs.Auth;

public class GerenteResponse
{
    public Guid PerfilId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal TaxaPlataforma { get; set; }
    public bool Gratuito { get; set; }
    public bool Ativo { get; set; }
}
