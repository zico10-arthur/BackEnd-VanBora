using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class VanRepository : IVanRepository
{
    private readonly AppDbContext _context;

    public VanRepository(AppDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Van?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vans
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<Van>> GetByGerenteUsuarioIdAsync(Guid gerenteUsuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Vans
            .AsNoTracking()
            .Where(v => v.GerenteUsuarioId == gerenteUsuarioId)
            .OrderBy(v => v.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<Van?> GetByIdAndGerenteAsync(Guid id, Guid gerenteUsuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Vans
            .FirstOrDefaultAsync(v => v.Id == id && v.GerenteUsuarioId == gerenteUsuarioId, cancellationToken);
    }

    public async Task AddAsync(Van van, CancellationToken cancellationToken = default)
    {
        await _context.Vans.AddAsync(van, cancellationToken);
    }

    public void Update(Van van)
    {
        var entry = _context.Entry(van);
        if (entry.State == EntityState.Detached)
            _context.Vans.Update(van);
    }
}
