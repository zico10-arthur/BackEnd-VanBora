using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByViagemVanIdAsync(Guid viagemVanId, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default);
    Task<List<int>> GetAssentosOcupadosAsync(Guid viagemVanId, CancellationToken cancellationToken = default);

    /// <summary>Mapa viagemVanId → assentos ocupados (evita N+1 na listagem pública).</summary>
    Task<IReadOnlyDictionary<Guid, List<int>>> GetAssentosOcupadosPorViagemVansAsync(
        IReadOnlyCollection<Guid> viagemVanIds,
        CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetExpiraveisAsync(CancellationToken cancellationToken = default);
    Task<int> CountReservasConfirmadasPorGerenteAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(Reserva reserva, CancellationToken cancellationToken = default);
    void Update(Reserva reserva);
}
