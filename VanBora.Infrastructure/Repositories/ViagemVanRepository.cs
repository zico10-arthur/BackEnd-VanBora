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
            .Include(vv => vv.MotoristaUsuario)
            .FirstOrDefaultAsync(vv => vv.Id == id, cancellationToken);
    }

    public async Task<List<ViagemVan>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default)
    {
        return await _context.ViagemVans
            .AsNoTracking()
            .Where(vv => vv.ViagemId == viagemId)
            .Include(vv => vv.Van)
            .Include(vv => vv.MotoristaUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ViagemVan>> GetByVanIdAsync(Guid vanId, CancellationToken cancellationToken = default)
    {
        return await _context.ViagemVans
            .AsNoTracking()
            .Where(vv => vv.VanId == vanId)
            .Include(vv => vv.Viagem)
            .Include(vv => vv.MotoristaUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ViagemVan viagemVan, CancellationToken cancellationToken = default)
    {
        await _context.ViagemVans.AddAsync(viagemVan, cancellationToken);
    }

    public void Remove(ViagemVan viagemVan)
    {
        var tracked = _context.ViagemVans.Local.FirstOrDefault(vv => vv.Id == viagemVan.Id);
        if (tracked is not null)
        {
            _context.ViagemVans.Remove(tracked);
        }
        else
        {
            _context.ViagemVans.Attach(viagemVan);
            _context.ViagemVans.Remove(viagemVan);
        }
    }
}
