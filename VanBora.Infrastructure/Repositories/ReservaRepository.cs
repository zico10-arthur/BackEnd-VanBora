using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private static readonly StatusReserva[] StatusAssentoOcupado =
    [
        StatusReserva.PendentePagamento,
        StatusReserva.Confirmada,
        StatusReserva.EmAndamento
    ];

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
            .ThenInclude(vv => vv.Viagem)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Reserva>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Include(r => r.Itens)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reserva>> GetByViagemVanIdAsync(Guid viagemVanId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Where(r => r.ViagemVanId == viagemVanId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reserva>> GetByViagemIdAsync(Guid viagemId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Where(r => r.ViagemVan.ViagemId == viagemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetAssentosOcupadosAsync(Guid viagemVanId, CancellationToken cancellationToken = default)
    {
        return await _context.ItemReservas
            .Where(i =>
                i.Reserva.ViagemVanId == viagemVanId &&
                StatusAssentoOcupado.Contains(i.Reserva.Status))
            .Select(i => i.NumeroAssento)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, List<int>>> GetAssentosOcupadosPorViagemVansAsync(
        IReadOnlyCollection<Guid> viagemVanIds,
        CancellationToken cancellationToken = default)
    {
        if (viagemVanIds.Count == 0)
            return new Dictionary<Guid, List<int>>();

        var pares = await _context.ItemReservas
            .Where(i =>
                viagemVanIds.Contains(i.Reserva.ViagemVanId) &&
                StatusAssentoOcupado.Contains(i.Reserva.Status))
            .Select(i => new { i.Reserva.ViagemVanId, i.NumeroAssento })
            .ToListAsync(cancellationToken);

        return pares
            .GroupBy(x => x.ViagemVanId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.NumeroAssento).Distinct().ToList());
    }

    public async Task<List<Reserva>> GetExpiraveisAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        return await _context.Reservas
            .Where(r => r.Status == StatusReserva.PendentePagamento && r.ExpiraEm <= agora)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountReservasConfirmadasPorGerenteAsync(
        Guid gerenteUsuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Where(r =>
                r.Status == StatusReserva.Confirmada &&
                r.ViagemVan.Viagem.GerenteUsuarioId == gerenteUsuarioId)
            .Select(r => r.UsuarioId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        await _context.Reservas.AddAsync(reserva, cancellationToken);
    }

    public void Update(Reserva reserva)
    {
        _context.Reservas.Update(reserva);
    }
}
