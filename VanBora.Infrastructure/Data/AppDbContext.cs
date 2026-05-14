using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data.Configurations;

namespace VanBora.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Evita descoberta transitiva de Van/Viagem/Reserva até existirem configurações dedicadas.
        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.Ignore(p => p.Vans);
            entity.Ignore(p => p.Viagens);
            entity.Ignore(p => p.ViagemVansDirigindo);
            entity.Ignore(p => p.MotoristasCriados);
        });

        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new PerfilConfiguration());

        // Demais configurações (Van, Viagem, etc.) serão adicionadas em Sprints futuras
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
