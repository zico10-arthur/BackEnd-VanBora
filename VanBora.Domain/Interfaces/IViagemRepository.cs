using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IViagemRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Viagem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Viagem?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Viagem>> GetByGerenteUsuarioIdAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task<List<Viagem>> GetDisponiveisAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Viagem viagem, CancellationToken cancellationToken = default);
    void Update(Viagem viagem);
}
