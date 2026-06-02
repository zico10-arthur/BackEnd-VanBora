using System.Data;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

    public Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default) =>
        _context.BeginTransactionAsync(isolationLevel, cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _context.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _context.RollbackAsync(cancellationToken);
    }
}
