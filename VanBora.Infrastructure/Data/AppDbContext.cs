using System.Data;
using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data.Configurations;

namespace VanBora.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Van> Vans => Set<Van>();
    public DbSet<Viagem> Viagens => Set<Viagem>();
    public DbSet<ViagemVan> ViagemVans => Set<ViagemVan>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ItemReserva> ItemReservas => Set<ItemReserva>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new VanConfiguration());
        modelBuilder.ApplyConfiguration(new ViagemConfiguration());
        modelBuilder.ApplyConfiguration(new ViagemVanConfiguration());
        modelBuilder.ApplyConfiguration(new ReservaConfiguration());
        modelBuilder.ApplyConfiguration(new ItemReservaConfiguration());
    }

    // Implementação de IUnitOfWork
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.RollbackTransactionAsync(cancellationToken);
    }
}
