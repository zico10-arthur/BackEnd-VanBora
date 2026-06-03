using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class ViagemRepository : IViagemRepository
{
    private readonly AppDbContext _context;

    public ViagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Viagem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Viagens
            .Include(v => v.ViagemVans)
            .ThenInclude(vv => vv.Van)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<Viagem>> GetByGerenteUsuarioIdAsync(
        Guid gerenteUsuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Viagens
            .Where(v => v.GerenteUsuarioId == gerenteUsuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Viagem>> GetDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        return await _context.Viagens
            .Include(v => v.ViagemVans)
            .ThenInclude(vv => vv.Van)
            .Where(v => v.Status == StatusViagem.Agendada && v.DataEvento >= agora)
            .OrderBy(v => v.DataEvento)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Viagem viagem, CancellationToken cancellationToken = default)
    {
        await _context.Viagens.AddAsync(viagem, cancellationToken);
    }

    public void Update(Viagem viagem)
    {
        _context.Viagens.Update(viagem);
    }
}
