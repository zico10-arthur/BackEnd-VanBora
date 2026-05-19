using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IVanRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Van?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Van>> GetByGerenteUsuarioIdAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task<Van?> GetByIdAndGerenteAsync(Guid id, Guid gerenteUsuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(Van van, CancellationToken cancellationToken = default);
    void Update(Van van);
}
