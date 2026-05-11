using VanBora.Domain.Entities;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByCpfAsync(CPF cpf, CancellationToken cancellationToken = default);
    Task<List<Usuario>> SearchAsync(string termo, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
    void Update(Usuario usuario);
}
