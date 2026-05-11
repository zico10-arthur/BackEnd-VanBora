using VanBora.Domain.Entities;

namespace VanBora.Domain.Interfaces;

public interface IViagemRepository
{
    Task<Viagem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Viagem>> GetByGerentePerfilIdAsync(Guid gerentePerfilId, CancellationToken cancellationToken = default);
    Task<List<Viagem>> GetDisponiveisAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Viagem viagem, CancellationToken cancellationToken = default);
    void Update(Viagem viagem);
}
