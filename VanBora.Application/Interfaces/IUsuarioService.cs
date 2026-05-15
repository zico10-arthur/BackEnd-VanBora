using VanBora.Domain.Common;
using VanBora.Domain.Entities;

namespace VanBora.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> BuscarPorCpfAsync(string cpf, CancellationToken cancellationToken = default);

    Task<Usuario?> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Result<Usuario>> ObterOuCriarAsync(
        string nome, string cpf, string email, string? telefone, string senha,
        CancellationToken cancellationToken = default);

    Task<Result<Usuario>> AtualizarContaPendenteAsync(
        Usuario usuario, string nome, string email, string? telefone, string senha,
        CancellationToken cancellationToken = default);
}
