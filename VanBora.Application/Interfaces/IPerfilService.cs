using VanBora.Domain.Common;
using VanBora.Domain.Entities;

namespace VanBora.Application.Interfaces;

public interface IPerfilService
{
    Task<Result<Perfil>> CriarPerfilGerenteAsync(Usuario usuario, string slug, CancellationToken cancellationToken = default);

    Perfil CriarPerfilPassageiro(Guid usuarioId);

    Task<(bool gratuito, decimal taxa)> CalcularTaxaAsync(CancellationToken cancellationToken = default);

    Task<bool> SlugJaEmUsoAsync(string slug, CancellationToken cancellationToken = default);

    Task<List<string>> BuscarPerfisAtivosAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
