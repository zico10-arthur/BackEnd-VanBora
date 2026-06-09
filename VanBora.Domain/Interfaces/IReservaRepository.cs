using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByViagemVanIdAsync(Guid viagemVanId, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default);
    Task<List<int>> GetAssentosOcupadosAsync(Guid viagemVanId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, List<int>>> GetAssentosOcupadosPorViagemVansAsync(List<Guid> viagemVanIds, CancellationToken cancellationToken = default);
    Task<bool> HasReservasAtivasByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<int> GetCountByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<Reserva>> GetReservasPendentesExpiradasAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Reserva reserva, CancellationToken cancellationToken = default);
    void Update(Reserva reserva);
}
