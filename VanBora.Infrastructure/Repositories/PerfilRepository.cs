using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class PerfilRepository : IPerfilRepository
{
    private readonly AppDbContext _context;

    public PerfilRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Perfil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Perfil>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Perfil>> GetByTipoAsync(TipoPerfil tipo, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Where(p => p.Tipo == tipo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Perfil?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Slug != null && p.Slug == slug, cancellationToken);
    }

    public async Task<List<Perfil>> GetGerentesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Where(p => p.Tipo == TipoPerfil.Gerente)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Perfil>> GetMotoristasByGerenteAsync(Guid gerentePerfilId, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Where(p => p.CriadoPorPerfilId == gerentePerfilId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Perfil>> SearchGerentesAsync(string search, CancellationToken cancellationToken = default)
    {
        return await _context.Perfis
            .Include(p => p.Usuario)
            .Where(p => p.Tipo == TipoPerfil.Gerente
                     && EF.Functions.ILike(p.Slug!, $"%{search}%"))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Perfil perfil, CancellationToken cancellationToken = default)
    {
        await _context.Perfis.AddAsync(perfil, cancellationToken);
    }

    public void Update(Perfil perfil)
    {
        _context.Perfis.Update(perfil);
    }
}
