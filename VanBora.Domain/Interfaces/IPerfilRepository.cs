using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Domain.Interfaces;

public interface IPerfilRepository
{
    Task<Perfil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Perfil>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<Perfil>> GetByTipoAsync(TipoPerfil tipo, CancellationToken cancellationToken = default);
    Task<Perfil?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Perfil>> GetGerentesAsync(CancellationToken cancellationToken = default);
    Task<List<Perfil>> GetMotoristasByGerenteAsync(Guid gerentePerfilId, CancellationToken cancellationToken = default);
    Task<List<Perfil>> SearchGerentesAsync(string search, CancellationToken cancellationToken = default);
    Task AddAsync(Perfil perfil, CancellationToken cancellationToken = default);
    void Update(Perfil perfil);
}
