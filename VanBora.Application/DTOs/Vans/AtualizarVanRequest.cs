namespace VanBora.Application.DTOs.Vans;

public sealed class AtualizarVanRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
}
