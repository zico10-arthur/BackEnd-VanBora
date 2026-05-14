using VanBora.Domain.Entities;

namespace VanBora.Application.Interfaces;

public interface ITokenService
{
    string GerarToken(Usuario usuario);

    string GerarToken(Guid usuarioId, string nome, string email, List<string> perfis);
}
