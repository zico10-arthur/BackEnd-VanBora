using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IVanRepository
{
    Task<Van?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Van>> GetByGerentePerfilIdAsync(Guid gerentePerfilId, CancellationToken cancellationToken = default);
    Task<Van?> GetByIdAndGerenteAsync(Guid id, Guid gerentePerfilId, CancellationToken cancellationToken = default);
    Task AddAsync(Van van, CancellationToken cancellationToken = default);
    void Update(Van van);
}
