using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByCpfAsync(CPF cpf, CancellationToken cancellationToken = default);
    Task<Usuario?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Usuario>> SearchAsync(string termo, CancellationToken cancellationToken = default);
    Task<int> CountByTipoAsync(TipoUsuario tipo, CancellationToken cancellationToken = default);
    Task<List<Usuario>> GetMotoristasByGerenteIdAsync(Guid gerenteId, CancellationToken cancellationToken = default);
    Task<List<Usuario>> GetGerentesAsync(string? search, CancellationToken cancellationToken = default);
    Task<List<Usuario>> SearchAllAsync(string? search, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
    void Update(Usuario usuario);
}
