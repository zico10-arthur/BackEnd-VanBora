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

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Reserva/ItemReserva serão adicionados em Sprints futuras
        modelBuilder.Ignore<ItemReserva>();
        modelBuilder.Ignore<Reserva>();

        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new VanConfiguration());
        modelBuilder.ApplyConfiguration(new ViagemConfiguration());
        modelBuilder.ApplyConfiguration(new ViagemVanConfiguration());
    }

    // Implementação de IUnitOfWork
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
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
