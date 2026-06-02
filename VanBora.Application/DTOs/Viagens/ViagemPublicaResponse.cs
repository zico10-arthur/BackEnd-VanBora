namespace VanBora.Application.DTOs.Viagens;

public sealed class ViagemVanPublicaResponse
{
    public Guid ViagemVanId { get; init; }
    public Guid ViagemId { get; init; }
    public string NomeVan { get; init; } = string.Empty;
    public string ModeloVan { get; init; } = string.Empty;
    public string PlacaVan { get; init; } = string.Empty;
    public int CapacidadePassageiros { get; init; }
    public int AssentosDisponiveis { get; init; }
}

public sealed class ViagemPublicaResponse
{
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; }
    public string LocalEvento { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public string LocalPartida { get; init; } = string.Empty;
    public decimal PrecoAssento { get; init; }
    public bool PossuiIngresso { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<ViagemVanPublicaResponse> Vans { get; init; } = [];
}

public sealed class ViagemVanDetalheResponse
{
    public Guid ViagemVanId { get; init; }
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public DateTime DataEvento { get; init; }
    public string LocalEvento { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public string LocalPartida { get; init; } = string.Empty;
    public decimal PrecoAssento { get; init; }
    public bool PossuiIngresso { get; init; }
    public string NomeVan { get; init; } = string.Empty;
    public string ModeloVan { get; init; } = string.Empty;
    public string PlacaVan { get; init; } = string.Empty;
    public int CapacidadePassageiros { get; init; }
    public List<int> AssentosOcupados { get; init; } = [];
}
