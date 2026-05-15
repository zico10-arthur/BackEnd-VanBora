using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;

namespace VanBora.Application.Services;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfilRepo;

    public PerfilService(IPerfilRepository perfilRepo)
    {
        _perfilRepo = perfilRepo;
    }

    public async Task<Result<Perfil>> CriarPerfilGerenteAsync(
        Usuario usuario, string slug, CancellationToken cancellationToken = default)
    {
        if (usuario.Perfis.Any(p => p.Tipo == TipoPerfil.Gerente))
            return Error.Conflict("GERENTE_EXISTENTE", "Usuário já possui perfil de gerente.");

        var slugNormalizado = slug.Trim().ToLowerInvariant();

        if (await SlugJaEmUsoAsync(slugNormalizado, cancellationToken))
            return Error.Conflict("SLUG_DUPLICADO", "Slug já cadastrado.");

        var (gratuito, taxa) = await CalcularTaxaAsync(cancellationToken);

        var perfil = Perfil.CriarGerente(usuario.Id, slugNormalizado, taxa, gratuito);
        usuario.AdicionarPerfil(perfil);
        await _perfilRepo.AddAsync(perfil, cancellationToken);

        return Result<Perfil>.Success(perfil);
    }

    public Perfil CriarPerfilPassageiro(Guid usuarioId)
    {
        return Perfil.CriarPassageiro(usuarioId);
    }

    public async Task<(bool gratuito, decimal taxa)> CalcularTaxaAsync(CancellationToken cancellationToken = default)
    {
        var gerentes = await _perfilRepo.GetByTipoAsync(TipoPerfil.Gerente, cancellationToken);
        var gratuito = gerentes.Count < 2;
        var taxa = gratuito ? 0m : 5.0m;
        return (gratuito, taxa);
    }

    public async Task<bool> SlugJaEmUsoAsync(string slug, CancellationToken cancellationToken = default)
    {
        var existente = await _perfilRepo.GetBySlugAsync(slug, cancellationToken);
        return existente is not null;
    }

    public async Task<List<string>> BuscarPerfisAtivosAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var perfis = await _perfilRepo.GetByUsuarioIdAsync(usuarioId, cancellationToken);
        return perfis
            .Where(p => p.Ativo)
            .Select(p => p.Tipo.ToString())
            .ToList();
    }
}
