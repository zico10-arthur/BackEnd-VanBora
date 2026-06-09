namespace VanBora.Application.DTOs.Viagens;

public class ViagemResponse
{
    public Guid Id { get; init; }
    public Guid GerenteUsuarioId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; }
    public string LocalEvento { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public string LocalPartida { get; init; } = string.Empty;
    public decimal PrecoAssento { get; init; }
    public int QuorumMinimo { get; init; }
    public bool PossuiIngresso { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
    public List<ViagemVanResponse> Vans { get; init; } = [];
}

public class ViagemVanResponse
{
    public Guid Id { get; init; }
    public Guid VanId { get; init; }
    public string NomeVan { get; init; } = string.Empty;
    public string PlacaVan { get; init; } = string.Empty;
    public int CapacidadeVan { get; init; }
    public int AssentosDisponiveis { get; init; }
    public Guid? MotoristaUsuarioId { get; init; }
}
