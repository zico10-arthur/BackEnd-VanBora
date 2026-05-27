using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Include(r => r.Itens)
            .Include(r => r.ViagemVan)
                .ThenInclude(vv => vv.Van)
            .Include(r => r.ViagemVan)
                .ThenInclude(vv => vv.Viagem)
                    .ThenInclude(v => v.GerenteUsuario)
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Reserva>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AsSplitQuery()
            .Where(r => r.UsuarioId == usuarioId)
            .Include(r => r.Itens)
            .Include(r => r.ViagemVan)
                .ThenInclude(vv => vv.Van)
            .Include(r => r.ViagemVan)
                .ThenInclude(vv => vv.Viagem)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reserva>> GetByViagemVanIdAsync(Guid viagemVanId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Where(r => r.ViagemVanId == viagemVanId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reserva>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AsSplitQuery()
            .Where(r => r.ViagemVan!.ViagemId == viagemId)
            .Include(r => r.ViagemVan)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetAssentosOcupadosAsync(Guid viagemVanId, CancellationToken cancellationToken = default)
    {
        return await _context.ItensReserva
            .AsNoTracking()
            .Where(i =>
                i.Reserva!.ViagemVanId == viagemVanId &&
                (i.Reserva.Status == StatusReserva.PendentePagamento ||
                 i.Reserva.Status == StatusReserva.Confirmada) &&
                i.Reserva.ExpiraEm >= DateTime.UtcNow)
            .Select(i => i.NumeroAssento)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasReservasAtivasByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r =>
                r.UsuarioId == usuarioId &&
                ((r.Status == StatusReserva.PendentePagamento && r.ExpiraEm >= DateTime.UtcNow) ||
                  r.Status == StatusReserva.Confirmada ||
                  r.Status == StatusReserva.EmAndamento), cancellationToken);
    }

    public async Task AddAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        await _context.Reservas.AddAsync(reserva, cancellationToken);
    }

    public void Update(Reserva reserva)
    {
        var entry = _context.Entry(reserva);
        if (entry.State == EntityState.Detached)
            _context.Reservas.Update(reserva);
    }
}
