namespace VanBora.Application.DTOs.Viagens;

public class ViagemGerenteResponse
{
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; }
    public string LocalEvento { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public string LocalPartida { get; init; } = string.Empty;
    public decimal PrecoAssento { get; init; }
    public int QuorumMinimo { get; init; }
    public bool PossuiIngresso { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Receita { get; init; }
    public int TotalReservas { get; init; }
    public List<ViagemVanGerenteInfo> Vans { get; init; } = [];
}

public class ViagemVanGerenteInfo
{
    public Guid ViagemVanId { get; init; }
    public string VanModelo { get; init; } = string.Empty;
    public string VanPlaca { get; init; } = string.Empty;
    public int Capacidade { get; init; }
    public int AssentosVendidos { get; init; }
    public string? MotoristaNome { get; init; }
}
