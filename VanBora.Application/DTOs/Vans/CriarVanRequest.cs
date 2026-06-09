namespace VanBora.Application.DTOs.Vans;

public sealed class CriarVanRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Capacidade { get; set; }
}
