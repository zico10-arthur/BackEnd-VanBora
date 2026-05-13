using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;
using VanBora.Infrastructure.Data;

namespace VanBora.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Perfis)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Perfis)
            .FirstOrDefaultAsync(u => u.Email != null && u.Email.Valor == email.Valor, cancellationToken);
    }

    public async Task<Usuario?> GetByCpfAsync(CPF cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Perfis)
            .FirstOrDefaultAsync(u => u.CPF.Valor == cpf.Valor, cancellationToken);
    }

    public async Task<List<Usuario>> SearchAsync(string termo, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Perfis)
            .Where(u => EF.Functions.ILike(u.Nome, $"%{termo}%"))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public void Update(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
    }
}
