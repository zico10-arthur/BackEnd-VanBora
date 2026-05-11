using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IViagemVanRepository
{
    Task<ViagemVan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ViagemVan>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default);
    Task<List<ViagemVan>> GetByVanIdAsync(Guid vanId, CancellationToken cancellationToken = default);
    Task AddAsync(ViagemVan viagemVan, CancellationToken cancellationToken = default);
    void Remove(ViagemVan viagemVan);
}
