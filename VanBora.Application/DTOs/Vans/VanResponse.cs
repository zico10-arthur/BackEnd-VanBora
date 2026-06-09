namespace VanBora.Application.DTOs.Vans;

public sealed class VanResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
