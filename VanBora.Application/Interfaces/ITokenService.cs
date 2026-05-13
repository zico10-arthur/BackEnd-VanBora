namespace VanBora.Application.Interfaces;

public interface ITokenService
{
    string GerarToken(Guid usuarioId, string nome, string email, List<string> perfis);
}
