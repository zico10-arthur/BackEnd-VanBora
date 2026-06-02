using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class ViagemVanRepository : IViagemVanRepository
{
    private readonly AppDbContext _context;

    public ViagemVanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ViagemVan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ViagemVans
            .Include(vv => vv.Van)
            .Include(vv => vv.Viagem)
            .ThenInclude(v => v.GerenteUsuario)
            .FirstOrDefaultAsync(vv => vv.Id == id, cancellationToken);
    }

    public async Task<List<ViagemVan>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default)
    {
        return await _context.ViagemVans
            .Where(vv => vv.ViagemId == viagemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ViagemVan>> GetByVanIdAsync(Guid vanId, CancellationToken cancellationToken = default)
    {
        return await _context.ViagemVans
            .Where(vv => vv.VanId == vanId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ViagemVan viagemVan, CancellationToken cancellationToken = default)
    {
        await _context.ViagemVans.AddAsync(viagemVan, cancellationToken);
    }

    public void Remove(ViagemVan viagemVan)
    {
        _context.ViagemVans.Remove(viagemVan);
    }
}
